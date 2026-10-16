using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/events")]
public class EventsController(IEventService events) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] EventType? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to) => Ok(await events.GetAllAsync(search, type, from, to));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => (await events.GetAsync(id)) is { } item ? Ok(item) : NotFound(new { message = "Event not found." });

    [Authorize(Roles = "Organiser"), HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var item = await events.CreateAsync(CurrentUserId, request);
        return CreatedAtAction(nameof(Get), new { id = item.EventId }, item);
    }

    [Authorize(Roles = "Organiser"), HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request)
    {
        var item = await events.UpdateAsync(CurrentUserId, id, request);
        return item is null ? NotFound(new { message = "Event not found or you are not the owner." }) : Ok(item);
    }

    [Authorize(Roles = "Organiser"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await events.DeleteAsync(CurrentUserId, id) ? Ok(new { message = "Event deleted." }) : NotFound(new { message = "Event not found or you are not the owner." });
}
