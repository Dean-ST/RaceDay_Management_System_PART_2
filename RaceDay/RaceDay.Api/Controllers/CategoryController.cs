using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoryController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoryController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/events/{eventId}/categories
    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        bool eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryDto(
                c.CategoryId,
                c.Age,
                c.EventId
            ))
            .ToListAsync();

        return Ok(categories);
    }

    // POST: api/events/{eventId}/categories
    [HttpPost("events/{eventId}/categories")]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        CreateCategoryRequest request)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        if (user.UserType != "Organiser" || user.OrganiserId == null)
        {
            return Forbid();
        }

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        if (raceEvent.OrganiserId != user.OrganiserId)
        {
            return Forbid();
        }

        if (request.Age < 1)
        {
            return BadRequest("Category age must be greater than zero.");
        }

        var category = new Category
        {
            Age = request.Age,
            EventId = eventId
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEventCategories),
            new { eventId = eventId },
            new CategoryDto(
                category.CategoryId,
                category.Age,
                category.EventId
            )
        );
    }

    // PUT: api/categories/{id}
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        CreateCategoryRequest request)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        if (user.UserType != "Organiser" || user.OrganiserId == null)
        {
            return Forbid();
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event == null ||
            category.Event.OrganiserId != user.OrganiserId)
        {
            return Forbid();
        }

        if (request.Age < 1)
        {
            return BadRequest("Category age must be greater than zero.");
        }

        category.Age = request.Age;

        await _context.SaveChangesAsync();

        return Ok("Category updated successfully.");
    }

    // DELETE: api/categories/{id}
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        if (user.UserType != "Organiser" || user.OrganiserId == null)
        {
            return Forbid();
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event == null ||
            category.Event.OrganiserId != user.OrganiserId)
        {
            return Forbid();
        }

        if (category.Enrollments.Any())
        {
            return Conflict(
                "This category cannot be deleted because participants are enrolled in it."
            );
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}