using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class ResultsController(IResultService results) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Organiser"), HttpPost("enrolments/{enrolmentId:int}/result")]
    public async Task<IActionResult> Create(int enrolmentId, CreateResultRequest request)
    {
        try { var item = await results.CreateAsync(CurrentUserId, enrolmentId, request); return Created("/api/results", item); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Participant"), HttpGet("results/my")]
    public async Task<IActionResult> Mine() => Ok(await results.GetMineAsync(CurrentUserId));

    [HttpGet("events/{eventId:int}/results")]
    public async Task<IActionResult> ForEvent(int eventId) => Ok(await results.GetForEventAsync(eventId) ?? Array.Empty<ResultResponse>());

    [Authorize(Roles = "Organiser"), HttpPut("results/{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateResultRequest request) => (await results.UpdateAsync(CurrentUserId, id, request)) is { } item ? Ok(item) : NotFound(new { message = "Result not found or you do not manage the event." });
}
