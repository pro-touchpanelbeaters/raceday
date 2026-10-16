using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

public class ResultServiceTests
{
    private static RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Organiser_Can_Record_Result_For_Own_Event()
    {
        await using var db = Db();
        var org = new User { FirstName = "Org", LastName = "O", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(-1), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var enrolment = new Enrolment { Participant = participant, Event = e, Category = c, EnrolmentDate = DateTime.UtcNow.AddDays(-2), Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org, participant); db.Events.Add(e); db.Categories.Add(c); db.Enrolments.Add(enrolment); await db.SaveChangesAsync();
        var service = new ResultService(db);
        var result = await service.CreateAsync(org.UserId, enrolment.EnrolmentId, new CreateResultRequest(TimeSpan.FromMinutes(48), 12));
        Assert.NotNull(result);
        Assert.Equal(12, result!.FinishingPosition);
    }

    [Fact]
    public async Task Other_Organiser_Cannot_Record_Result()
    {
        await using var db = Db();
        var owner = new User { FirstName = "Owner", LastName = "O", Email = "owner@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var other = new User { FirstName = "Other", LastName = "O", Email = "other@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = owner, Name = "Race", EventDate = DateTime.UtcNow.AddDays(-1), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var enrolment = new Enrolment { Participant = participant, Event = e, Category = c, EnrolmentDate = DateTime.UtcNow.AddDays(-2), Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(owner, other, participant); db.Events.Add(e); db.Categories.Add(c); db.Enrolments.Add(enrolment); await db.SaveChangesAsync();
        var service = new ResultService(db);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(other.UserId, enrolment.EnrolmentId, new CreateResultRequest(TimeSpan.FromMinutes(50), 20)));
    }

    [Fact]
    public async Task Participant_Only_Sees_Published_Results()
    {
        await using var db = Db();
        var org = new User { FirstName = "Org", LastName = "O", Email = "org@test.com", PasswordHash = "x", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant = new User { FirstName = "Part", LastName = "P", Email = "part@test.com", PasswordHash = "x", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e = new Event { Organiser = org, Name = "Race", EventDate = DateTime.UtcNow.AddDays(-1), Location = "Joburg", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var c = new Category { Event = e, Name = "Senior", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var en = new Enrolment { Participant = participant, Event = e, Category = c, EnrolmentDate = DateTime.UtcNow.AddDays(-2), Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(org, participant); db.Events.Add(e); db.Categories.Add(c); db.Enrolments.Add(en); await db.SaveChangesAsync();
        db.Results.Add(new Result { EnrolmentId = en.EnrolmentId, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 5, PublishedAt = null, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var service = new ResultService(db);
        Assert.Empty(await service.GetMineAsync(participant.UserId));
    }
}
