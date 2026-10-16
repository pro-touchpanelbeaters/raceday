using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Services;

public class ResultService(RaceDayDbContext db) : IResultService
{
    private static IQueryable<ResultResponse> Projection(IQueryable<Result> q) => q.Select(x => new ResultResponse(x.ResultId, x.EnrolmentId, x.Enrolment.Participant.FirstName + " " + x.Enrolment.Participant.LastName, x.Enrolment.Event.Name, x.Enrolment.Event.EventDate, x.Enrolment.Category.Name, x.FinishTime, x.FinishingPosition, x.PublishedAt));

    public async Task<ResultResponse?> CreateAsync(int organiserId, int enrolmentId, CreateResultRequest request)
    {
        var enrolment = await db.Enrolments.Include(x => x.Event).SingleOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (enrolment is null) throw new KeyNotFoundException("Enrolment not found.");
        if (enrolment.Event.OrganiserId != organiserId) throw new UnauthorizedAccessException("You do not manage this event.");
        if (await db.Results.AnyAsync(x => x.EnrolmentId == enrolmentId)) throw new InvalidOperationException("A result already exists for this enrolment.");
        var result = new Result { EnrolmentId = enrolmentId, FinishTime = request.FinishTime, FinishingPosition = request.FinishingPosition, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Results.Add(result); await db.SaveChangesAsync();
        return await Projection(db.Results.AsNoTracking().Where(x => x.ResultId == result.ResultId)).SingleAsync();
    }

    public async Task<IReadOnlyList<ResultResponse>> GetMineAsync(int participantId) => await Projection(db.Results.AsNoTracking().Where(x => x.Enrolment.ParticipantId == participantId && x.PublishedAt != null).OrderByDescending(x => x.Enrolment.Event.EventDate)).ToListAsync();

    public async Task<IReadOnlyList<ResultResponse>?> GetForEventAsync(int eventId)
    {
        if (!await db.Events.AnyAsync(x => x.EventId == eventId)) return null;
        return await Projection(db.Results.AsNoTracking().Where(x => x.Enrolment.EventId == eventId).OrderBy(x => x.FinishingPosition)).ToListAsync();
    }

    public async Task<ResultResponse?> UpdateAsync(int organiserId, int resultId, UpdateResultRequest request)
    {
        var result = await db.Results.Include(x => x.Enrolment).ThenInclude(x => x.Event).SingleOrDefaultAsync(x => x.ResultId == resultId);
        if (result is null || result.Enrolment.Event.OrganiserId != organiserId) return null;
        result.FinishTime = request.FinishTime; result.FinishingPosition = request.FinishingPosition; result.PublishedAt = request.Publish ? DateTime.UtcNow : null; result.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return await Projection(db.Results.AsNoTracking().Where(x => x.ResultId == resultId)).SingleAsync();
    }
}
