using System.Security.Claims;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Middleware;

public class SessionAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.Session.GetInt32("UserId");
        var role = context.Session.GetString("Role");
        if (userId.HasValue && !string.IsNullOrWhiteSpace(role))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, context.Session.GetString("UserEmail") ?? userId.Value.ToString())
            };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "RaceDaySession"));
        }
        await next(context);
    }
}
