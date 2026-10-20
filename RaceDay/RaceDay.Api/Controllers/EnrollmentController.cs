using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrollmentController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrollmentController(RaceDayDbContext context)
    {
        _context = context;
    }

    // POST: api/enrolments
    [HttpPost]
    public async Task<IActionResult> CreateEnrollment(
        CreateEnrolmentRequest request)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
            return Unauthorized("Please log in first.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Unauthorized();

        if (user.UserType != "Participant" ||
            user.ParticipantId == null)
            return Forbid();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == request.EventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

        if (category == null)
            return NotFound("Category not found.");

        // Ensure the category belongs to the selected event.
        if (category.EventId != request.EventId)
            return BadRequest(
                "The selected category does not belong to this event."
            );

        if (raceEvent.Date <= DateTime.Now)
            return BadRequest("You cannot enrol in an event that has already started.");

        bool alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e =>
                e.ParticipantId == user.ParticipantId &&
                e.EventId == request.EventId);

        if (alreadyEnrolled)
            return Conflict("You are already enrolled in this event.");

        var enrollment = new Enrollment
        {
            ParticipantId = user.ParticipantId.Value,
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            Status = "Pending"
        };

        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync();

        return Created(
            $"/api/enrolments/{enrollment.EnrollmentId}",
            new
            {
                enrollment.EnrollmentId,
                enrollment.ParticipantId,
                enrollment.EventId,
                enrollment.CategoryId,
                enrollment.Status
            }
        );
    }

    // GET: api/enrolments/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
            return Unauthorized("Please log in first.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Unauthorized();

        if (user.UserType != "Participant" ||
            user.ParticipantId == null)
            return Forbid();

        var enrollments = await _context.Enrollments
            .Where(e => e.ParticipantId == user.ParticipantId)
            .Select(e => new EnrolmentDto(
                e.EnrollmentId,
                e.ParticipantId,
                e.Participant!.Name + " " + e.Participant.Surname,
                e.EventId,
                e.Event!.Name,
                e.CategoryId,
                e.Category!.Age,
                e.Status
            ))
            .ToListAsync();

        return Ok(enrollments);
    }

    // GET: api/enrolments/event/{eventId}
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventEnrollments(int eventId)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
            return Unauthorized("Please log in first.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Unauthorized();

        if (user.UserType != "Organiser" ||
            user.OrganiserId == null)
            return Forbid();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserId != user.OrganiserId)
            return Forbid();

        var enrollments = await _context.Enrollments
            .Where(e => e.EventId == eventId)
            .Select(e => new EnrolmentDto(
                e.EnrollmentId,
                e.ParticipantId,
                e.Participant!.Name + " " + e.Participant.Surname,
                e.EventId,
                e.Event!.Name,
                e.CategoryId,
                e.Category!.Age,
                e.Status
            ))
            .ToListAsync();

        return Ok(enrollments);
    }

    // PUT: api/enrolments/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateEnrollmentStatus(
        int id,
        UpdateEnrolmentStatusRequest request)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
            return Unauthorized("Please log in first.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return Unauthorized();

        if (user.UserType != "Organiser" ||
            user.OrganiserId == null)
            return Forbid();

        var enrollment = await _context.Enrollments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
            return NotFound("Enrollment not found.");

        if (enrollment.Event!.OrganiserId != user.OrganiserId)
            return Forbid();

        string status = request.Status.Trim();

        if (status != "Pending" &&
            status != "Confirmed" &&
            status != "Cancelled")
        {
            return BadRequest("Invalid enrollment status.");
        }

        enrollment.Status = status;

        await _context.SaveChangesAsync();

        return Ok("Enrollment status updated successfully.");
    }
}