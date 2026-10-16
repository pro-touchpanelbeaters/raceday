using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Services;

public class EventService(RaceDayDbContext db) : IEventService
{
    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(string? search, EventType? type, DateTime? from, DateTime? to)
    {
        var q = db.Events.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search) || x.Location.Contains(search));
        if (type.HasValue) q = q.Where(x => x.EventType == type);
        if (from.HasValue) q = q.Where(x => x.EventDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.EventDate <= to.Value);
        return await q.OrderBy(x => x.EventDate).Select(x => new EventResponse(x.EventId, x.OrganiserId, x.Name, x.Description, x.EventDate, x.Location, x.Distance, x.EventType, x.BannerImageUrl, x.Enrolments.Count)).ToListAsync();
    }

    public async Task<EventResponse?> GetAsync(int id) => await db.Events.AsNoTracking().Where(x => x.EventId == id).Select(x => new EventResponse(x.EventId, x.OrganiserId, x.Name, x.Description, x.EventDate, x.Location, x.Distance, x.EventType, x.BannerImageUrl, x.Enrolments.Count)).SingleOrDefaultAsync();

    public async Task<EventResponse> CreateAsync(int organiserId, CreateEventRequest request)
    {
        var entity = new Event { OrganiserId = organiserId, Name = request.Name.Trim(), Description = request.Description, EventDate = request.EventDate, Location = request.Location.Trim(), Distance = request.Distance, EventType = request.EventType, BannerImageUrl = request.BannerImageUrl, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Events.Add(entity); await db.SaveChangesAsync();
        return new EventResponse(entity.EventId, entity.OrganiserId, entity.Name, entity.Description, entity.EventDate, entity.Location, entity.Distance, entity.EventType, entity.BannerImageUrl, 0);
    }

    public async Task<EventResponse?> UpdateAsync(int organiserId, int id, UpdateEventRequest request)
    {
        var entity = await db.Events.FindAsync(id);
        if (entity is null || entity.OrganiserId != organiserId) return null;
        entity.Name = request.Name.Trim(); entity.Description = request.Description; entity.EventDate = request.EventDate; entity.Location = request.Location.Trim(); entity.Distance = request.Distance; entity.EventType = request.EventType; entity.BannerImageUrl = request.BannerImageUrl; entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<bool> DeleteAsync(int organiserId, int id)
    {
        var entity = await db.Events.FindAsync(id);
        if (entity is null || entity.OrganiserId != organiserId) return false;
        db.Events.Remove(entity); await db.SaveChangesAsync(); return true;
    }
}
