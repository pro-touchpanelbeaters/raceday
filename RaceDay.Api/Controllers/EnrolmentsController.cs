using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class EnrolmentsController(IEnrolmentService enrolments) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Participant"), HttpPost("events/{eventId:int}/enrolments")]
    public async Task<IActionResult> Create(int eventId, CreateEnrolmentRequest request)
    {
        try { var item = await enrolments.CreateAsync(CurrentUserId, eventId, request); return Created("/api/enrolments/my", item); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Participant"), HttpGet("enrolments/my")]
    public async Task<IActionResult> Mine() => Ok(await enrolments.GetMineAsync(CurrentUserId));

    [Authorize(Roles = "Organiser"), HttpGet("events/{eventId:int}/enrolments")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var result = await enrolments.GetForEventAsync(CurrentUserId, eventId);
        return result is null ? NotFound(new { message = "Event not found or you are not the owner." }) : Ok(result);
    }

    [Authorize(Roles = "Participant"), HttpDelete("enrolments/{enrolmentId:int}")]
    public async Task<IActionResult> Cancel(int enrolmentId) => await enrolments.CancelAsync(CurrentUserId, enrolmentId) ? Ok(new { message = "Enrolment cancelled." }) : NotFound();
}
