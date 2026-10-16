using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

public class EventServiceTests
{
    private static RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Organiser_Can_Create_Event()
    {
        await using var db = Db();
        var user = new User { FirstName = "Org", LastName = "One", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.Add(user); await db.SaveChangesAsync();
        var service = new EventService(db);
        var result = await service.CreateAsync(user.UserId, new CreateEventRequest("10K", "Race", DateTime.UtcNow.AddDays(10), "Joburg", 10, EventType.Run, null));
        Assert.Equal("10K", result.Name);
        Assert.Equal(user.UserId, result.OrganiserId);
    }

    [Fact]
    public async Task NonOwner_Cannot_Update_Event()
    {
        await using var db = Db();
        var org1 = new User { FirstName = "A", LastName = "A", Email = "a@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var org2 = new User { FirstName = "B", LastName = "B", Email = "b@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org1, org2);
        var e = new Event { Organiser = org1, Name = "Race", EventDate = DateTime.UtcNow.AddDays(10), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Events.Add(e); await db.SaveChangesAsync();
        var service = new EventService(db);
        var result = await service.UpdateAsync(org2.UserId, e.EventId, new UpdateEventRequest("Changed", null, e.EventDate, e.Location, 10, EventType.Run, null));
        Assert.Null(result);
    }

    [Fact]
    public async Task Organiser_Can_Delete_Own_Event()
    {
        await using var db = Db();
        var org = new User { FirstName = "A", LastName = "A", Email = "a@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.Add(org);
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(10), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Events.Add(e); await db.SaveChangesAsync();
        var service = new EventService(db);
        Assert.True(await service.DeleteAsync(org.UserId, e.EventId));
        Assert.False(await db.Events.AnyAsync());
    }
}
