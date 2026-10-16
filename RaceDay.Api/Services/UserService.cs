using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public class UserService(RaceDayDbContext db) : IUserService
{
    public async Task<UserResponse?> GetAsync(int userId) => await db.Users.AsNoTracking().Where(x => x.UserId == userId).Select(x => new UserResponse(x.UserId, x.FirstName, x.LastName, x.Email, x.PhoneNumber, x.Role, x.ProfileImageUrl)).SingleOrDefaultAsync();

    public async Task<UserResponse?> UpdateAsync(int userId, UpdateProfileRequest request)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;
        user.FirstName = request.FirstName.Trim(); user.LastName = request.LastName.Trim(); user.PhoneNumber = request.PhoneNumber; user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return AuthService.ToResponse(user);
    }
}
