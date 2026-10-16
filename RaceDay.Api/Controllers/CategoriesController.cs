using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class CategoriesController(ICategoryService categories) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetByEvent(int eventId) => Ok(await categories.GetByEventAsync(eventId));

    [HttpGet("categories/{id:int}")]
    public async Task<IActionResult> Get(int id) => (await categories.GetAsync(id)) is { } item ? Ok(item) : NotFound(new { message = "Category not found." });

    [Authorize(Roles = "Organiser"), HttpPost("events/{eventId:int}/categories")]
    public async Task<IActionResult> Create(int eventId, CreateCategoryRequest request)
    {
        try { var item = await categories.CreateAsync(CurrentUserId, eventId, request); return CreatedAtAction(nameof(Get), new { id = item.CategoryId }, item); }
        catch (UnauthorizedAccessException ex) { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Organiser"), HttpPut("categories/{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        try { var item = await categories.UpdateAsync(CurrentUserId, id, request); return item is null ? NotFound() : Ok(item); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Organiser"), HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> Delete(int id) => await categories.DeleteAsync(CurrentUserId, id) ? Ok(new { message = "Category deleted." }) : NotFound();
}
