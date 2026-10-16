using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;
using RaceDay.Api.Services;

namespace RaceDay.Tests;

public class AuthServiceTests
{
    private static RaceDayDbContext Db() => new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Register_Stores_Hashed_Password_And_Role()
    {
        await using var db = Db();
        var service = new AuthService(db);
        var result = await service.RegisterAsync(new RegisterRequest("Test", "User", "test@example.com", "Password123!", "0712345678", UserRole.Participant));
        var user = await db.Users.SingleAsync();
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(UserRole.Participant, result.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));
    }

    [Fact]
    public async Task Register_Duplicate_Email_Throws_Conflict_Error()
    {
        await using var db = Db();
        var service = new AuthService(db);
        var request = new RegisterRequest("Test", "User", "test@example.com", "Password123!", null, UserRole.Participant);
        await service.RegisterAsync(request);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task Login_Returns_User_For_Valid_Credentials()
    {
        await using var db = Db();
        var service = new AuthService(db);
        await service.RegisterAsync(new RegisterRequest("Test", "User", "test@example.com", "Password123!", null, UserRole.Organiser));
        var result = await service.LoginAsync(new LoginRequest("test@example.com", "Password123!"));
        Assert.NotNull(result);
        Assert.Equal(UserRole.Organiser, result!.Role);
    }

    [Fact]
    public async Task Login_Returns_Null_For_Invalid_Password()
    {
        await using var db = Db();
        var service = new AuthService(db);
        await service.RegisterAsync(new RegisterRequest("Test", "User", "test@example.com", "Password123!", null, UserRole.Participant));
        var result = await service.LoginAsync(new LoginRequest("test@example.com", "WrongPassword!"));
        Assert.Null(result);
    }
}
