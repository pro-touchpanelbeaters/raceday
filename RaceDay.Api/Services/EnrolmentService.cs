using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Services;

public class EnrolmentService(RaceDayDbContext db) : IEnrolmentService
{
    private static IQueryable<EnrolmentResponse> Projection(IQueryable<Enrolment> q) => q.Select(x => new EnrolmentResponse(x.EnrolmentId, x.ParticipantId, x.Participant.FirstName + " " + x.Participant.LastName, x.EventId, x.Event.Name, x.CategoryId, x.Category.Name, x.EnrolmentDate, x.Status));

    public async Task<EnrolmentResponse?> CreateAsync(int participantId, int eventId, CreateEnrolmentRequest request)
    {
        var category = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.CategoryId == request.CategoryId && x.EventId == eventId);
        if (category is null) throw new KeyNotFoundException("Category does not exist for this event.");
        if (category.Event.EventDate < DateTime.UtcNow) throw new InvalidOperationException("Cannot enrol in an event that has already taken place.");
        if (await db.Enrolments.AnyAsync(x => x.ParticipantId == participantId && x.EventId == eventId && x.Status != EnrolmentStatus.Cancelled)) throw new InvalidOperationException("You are already enrolled in this event.");
        var entity = new Enrolment { ParticipantId = participantId, EventId = eventId, CategoryId = request.CategoryId, EnrolmentDate = DateTime.UtcNow, Status = EnrolmentStatus.Confirmed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Enrolments.Add(entity); await db.SaveChangesAsync();
        return await Projection(db.Enrolments.AsNoTracking().Where(x => x.EnrolmentId == entity.EnrolmentId)).SingleAsync();
    }

    public async Task<IReadOnlyList<EnrolmentResponse>> GetMineAsync(int participantId) => await Projection(db.Enrolments.AsNoTracking().Where(x => x.ParticipantId == participantId).OrderByDescending(x => x.EnrolmentDate)).ToListAsync();

    public async Task<IReadOnlyList<EnrolmentResponse>?> GetForEventAsync(int organiserId, int eventId)
    {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == organiserId)) return null;
        return await Projection(db.Enrolments.AsNoTracking().Where(x => x.EventId == eventId).OrderBy(x => x.EnrolmentDate)).ToListAsync();
    }

    public async Task<bool> CancelAsync(int participantId, int enrolmentId)
    {
        var entity = await db.Enrolments.SingleOrDefaultAsync(x => x.EnrolmentId == enrolmentId && x.ParticipantId == participantId);
        if (entity is null) return false;
        if (entity.Status == EnrolmentStatus.Cancelled) return true;
        entity.Status = EnrolmentStatus.Cancelled; entity.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); return true;
    }
}
