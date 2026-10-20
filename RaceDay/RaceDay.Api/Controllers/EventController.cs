using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/events
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events
            .Select(e => new EventDto(
                e.EventId,
                e.Name,
                e.Description,
                e.Date,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserId
            ))
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        var result = new EventDto(
            raceEvent.EventId,
            raceEvent.Name,
            raceEvent.Description,
            raceEvent.Date,
            raceEvent.Location,
            raceEvent.Distance,
            raceEvent.EventType,
            raceEvent.OrganiserId
        );

        return Ok(result);
    }

    // GET: api/events/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyEvents()
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

        var events = await _context.Events
            .Where(e => e.OrganiserId == user.OrganiserId)
            .Select(e => new EventDto(
                e.EventId,
                e.Name,
                e.Description,
                e.Date,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserId
            ))
            .ToListAsync();

        return Ok(events);
    }

    // POST: api/events
    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        CreateEventRequest request)
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

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.EventType) ||
            request.Distance <= 0 ||
            request.Date == default)
        {
            return BadRequest("Please provide valid event information.");
        }

        var raceEvent = new RaceEvent
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Date = request.Date,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = request.EventType.Trim(),
            OrganiserId = user.OrganiserId.Value
        };

        _context.Events.Add(raceEvent);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EventId },
            new EventDto(
                raceEvent.EventId,
                raceEvent.Name,
                raceEvent.Description,
                raceEvent.Date,
                raceEvent.Location,
                raceEvent.Distance,
                raceEvent.EventType,
                raceEvent.OrganiserId
            )
        );
    }

    // PUT: api/events/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        UpdateEventRequest request)
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
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        if (raceEvent.OrganiserId != user.OrganiserId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.EventType) ||
            request.Distance <= 0 ||
            request.Date == default)
        {
            return BadRequest("Please provide valid event information.");
        }

        raceEvent.Name = request.Name.Trim();
        raceEvent.Description = request.Description?.Trim() ?? string.Empty;
        raceEvent.Date = request.Date;
        raceEvent.Location = request.Location.Trim();
        raceEvent.Distance = request.Distance;
        raceEvent.EventType = request.EventType.Trim();

        await _context.SaveChangesAsync();

        return Ok("Event updated successfully.");
    }

    // DELETE: api/events/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
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
            .Include(e => e.Categories)
            .Include(e => e.Enrollments)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        if (raceEvent.OrganiserId != user.OrganiserId)
        {
            return Forbid();
        }

        if (raceEvent.Categories.Any() || raceEvent.Enrollments.Any())
        {
            return Conflict(
                "This event cannot be deleted because it has categories or enrolments."
            );
        }

        _context.Events.Remove(raceEvent);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}