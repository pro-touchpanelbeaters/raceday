using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

public class EnrolmentServiceTests
{
    private static RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Participant_Can_Enrol_In_Event_Category()
    {
        await using var db = Db();
        var org = new User { FirstName = "Org", LastName = "O", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(10), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", MinAge = 18, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org, participant); db.Events.Add(e); db.Categories.Add(c); await db.SaveChangesAsync();
        var service = new EnrolmentService(db);
        var result = await service.CreateAsync(participant.UserId, e.EventId, new CreateEnrolmentRequest(c.CategoryId));
        Assert.NotNull(result);
        Assert.Equal(participant.UserId, result!.ParticipantId);
        Assert.Equal(c.CategoryId, result.CategoryId);
    }

    [Fact]
    public async Task Participant_Cannot_Enrol_Twice()
    {
        await using var db = Db();
        var org = new User { FirstName = "Org", LastName = "O", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(10), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org, participant); db.Events.Add(e); db.Categories.Add(c); await db.SaveChangesAsync();
        var service = new EnrolmentService(db);
        await service.CreateAsync(participant.UserId, e.EventId, new CreateEnrolmentRequest(c.CategoryId));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(participant.UserId, e.EventId, new CreateEnrolmentRequest(c.CategoryId)));
    }

    [Fact]
    public async Task Participant_Can_Cancel_Their_Enrolment()
    {
        await using var db = Db();
        var org = new User { FirstName = "Org", LastName = "O", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(10), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org, participant); db.Events.Add(e); db.Categories.Add(c); await db.SaveChangesAsync();
        var service = new EnrolmentService(db);
        var item = await service.CreateAsync(participant.UserId, e.EventId, new CreateEnrolmentRequest(c.CategoryId));
        Assert.True(await service.CancelAsync(participant.UserId, item!.EnrolmentId));
        Assert.Equal(EnrolmentStatus.Cancelled, (await db.Enrolments.SingleAsync()).Status);
    }
}
