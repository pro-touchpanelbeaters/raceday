using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RaceDayDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Users.AnyAsync()) return;

        var organiser1 = new User { FirstName = "Thabo", LastName = "Mokoena", Email = "thabo.organiser@raceday.co.za", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organiser123!"), PhoneNumber = "0710000001", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var organiser2 = new User { FirstName = "John", LastName = "Doe", Email = "john.organiser@raceday.co.za", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organiser123!"), PhoneNumber = "0710000002", Role = UserRole.Organiser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant1 = new User { FirstName = "Bulelani", LastName = "Mlindelwa", Email = "participant1@raceday.co.za", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Participant123!"), PhoneNumber = "0710000003", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var participant2 = new User { FirstName = "Ayanda", LastName = "Ndlovu", Email = "participant2@raceday.co.za", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Participant123!"), PhoneNumber = "0710000004", Role = UserRole.Participant, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.AddRange(organiser1, organiser2, participant1, participant2); await db.SaveChangesAsync();

        var e1 = new Event { OrganiserId = organiser1.UserId, Name = "Johannesburg Spring 10K", Description = "Road running event through Johannesburg.", EventDate = DateTime.UtcNow.AddMonths(1), Location = "Johannesburg, Gauteng", Distance = 10, EventType = EventType.Run, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e2 = new Event { OrganiserId = organiser1.UserId, Name = "Soweto Community Walk", Description = "Community walking event.", EventDate = DateTime.UtcNow.AddMonths(2), Location = "Soweto, Gauteng", Distance = 8, EventType = EventType.Walk, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var e3 = new Event { OrganiserId = organiser2.UserId, Name = "Cradle Cycle Challenge", Description = "Cycling challenge around the Cradle of Humankind.", EventDate = DateTime.UtcNow.AddMonths(3), Location = "Muldersdrift, Gauteng", Distance = 42, EventType = EventType.Cycle, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Events.AddRange(e1, e2, e3); await db.SaveChangesAsync();

        var cats = new[]
        {
            new Category { EventId = e1.EventId, Name = "Senior 10K", Description = "Open senior category", MinAge = 18, MaxAge = null, Distance = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { EventId = e1.EventId, Name = "Under 20", Description = "Junior category", MinAge = 13, MaxAge = 19, Distance = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { EventId = e2.EventId, Name = "Open Walk", Description = "All participants", MinAge = 12, MaxAge = null, Distance = 8, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Category { EventId = e3.EventId, Name = "42km Open", Description = "Open cycling category", MinAge = 18, MaxAge = null, Distance = 42, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        db.Categories.AddRange(cats); await db.SaveChangesAsync();

        var en1 = new Enrolment { ParticipantId = participant1.UserId, EventId = e1.EventId, CategoryId = cats[0].CategoryId, EnrolmentDate = DateTime.UtcNow, Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var en2 = new Enrolment { ParticipantId = participant2.UserId, EventId = e1.EventId, CategoryId = cats[1].CategoryId, EnrolmentDate = DateTime.UtcNow, Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var en3 = new Enrolment { ParticipantId = participant1.UserId, EventId = e2.EventId, CategoryId = cats[2].CategoryId, EnrolmentDate = DateTime.UtcNow, Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Enrolments.AddRange(en1, en2, en3); await db.SaveChangesAsync();
    }
}
