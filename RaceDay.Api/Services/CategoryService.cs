using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public class CategoryService(RaceDayDbContext db) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetByEventAsync(int eventId) => await db.Categories.AsNoTracking().Where(x => x.EventId == eventId).OrderBy(x => x.Name).Select(ToDto()).ToListAsync();
    public async Task<CategoryResponse?> GetAsync(int id) => await db.Categories.AsNoTracking().Where(x => x.CategoryId == id).Select(ToDto()).SingleOrDefaultAsync();

    public async Task<CategoryResponse> CreateAsync(int organiserId, int eventId, CreateCategoryRequest request)
    {
        var owns = await db.Events.AnyAsync(x => x.EventId == eventId && x.OrganiserId == organiserId);
        if (!owns) throw new UnauthorizedAccessException("You do not own this event.");
        if (request.MinAge.HasValue && request.MaxAge.HasValue && request.MinAge > request.MaxAge) throw new ArgumentException("Minimum age cannot exceed maximum age.");
        if (await db.Categories.AnyAsync(x => x.EventId == eventId && x.Name == request.Name)) throw new InvalidOperationException("Category already exists for this event.");
        var entity = new RaceDay.Api.Entities.Category { EventId = eventId, Name = request.Name.Trim(), Description = request.Description, MinAge = request.MinAge, MaxAge = request.MaxAge, Distance = request.Distance, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Categories.Add(entity); await db.SaveChangesAsync(); return new CategoryResponse(entity.CategoryId, entity.EventId, entity.Name, entity.Description, entity.MinAge, entity.MaxAge, entity.Distance);
    }

    public async Task<CategoryResponse?> UpdateAsync(int organiserId, int id, UpdateCategoryRequest request)
    {
        var entity = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.CategoryId == id);
        if (entity is null || entity.Event.OrganiserId != organiserId) return null;
        if (request.MinAge.HasValue && request.MaxAge.HasValue && request.MinAge > request.MaxAge) throw new ArgumentException("Minimum age cannot exceed maximum age.");
        entity.Name = request.Name.Trim(); entity.Description = request.Description; entity.MinAge = request.MinAge; entity.MaxAge = request.MaxAge; entity.Distance = request.Distance; entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return new CategoryResponse(entity.CategoryId, entity.EventId, entity.Name, entity.Description, entity.MinAge, entity.MaxAge, entity.Distance);
    }

    public async Task<bool> DeleteAsync(int organiserId, int id)
    {
        var entity = await db.Categories.Include(x => x.Event).SingleOrDefaultAsync(x => x.CategoryId == id);
        if (entity is null || entity.Event.OrganiserId != organiserId) return false;
        db.Categories.Remove(entity); await db.SaveChangesAsync(); return true;
    }

    private static System.Linq.Expressions.Expression<Func<Entities.Category, CategoryResponse>> ToDto() => x => new CategoryResponse(x.CategoryId, x.EventId, x.Name, x.Description, x.MinAge, x.MaxAge, x.Distance);
}
