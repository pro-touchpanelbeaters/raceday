using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try { return Created("/api/users/me", await auth.RegisterAsync(request)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await auth.LoginAsync(request);
        if (user is null) return Unauthorized(new { message = "Invalid email or password." });
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email);
        return Ok(user);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully." });
    }
}
