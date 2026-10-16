using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RaceDay.Api.Middleware;

// The signed-in user is set from the session by SessionAuthenticationMiddleware.
// This scheme only exists so [Authorize] can answer with 401/403 instead of throwing.
public class SessionAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "RaceDaySession";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(Context.User.Identity?.IsAuthenticated == true
            ? AuthenticateResult.Success(new AuthenticationTicket(Context.User, SchemeName))
            : AuthenticateResult.NoResult());

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Response.WriteAsJsonAsync(new { message = "You must be logged in." });
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsJsonAsync(new { message = "You do not have permission to perform this action." });
    }
}
