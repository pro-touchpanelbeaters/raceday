using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/users"), Authorize]
public class UsersController(IUserService users) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetMe() => (await users.GetAsync(CurrentUserId)) is { } user ? Ok(user) : NotFound();

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request) => (await users.UpdateAsync(CurrentUserId, request)) is { } user ? Ok(user) : NotFound();
}
