using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Services;

public class AuthService(RaceDayDbContext db) : IAuthService
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Role is not (UserRole.Organiser or UserRole.Participant))
            throw new ArgumentException("Invalid role.");
        if (await db.Users.AnyAsync(x => x.Email == email))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), PhoneNumber = request.PhoneNumber,
            Role = request.Role, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UserResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;
        return ToResponse(user);
    }

    internal static UserResponse ToResponse(User u) => new(u.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.Role, u.ProfileImageUrl);
}
