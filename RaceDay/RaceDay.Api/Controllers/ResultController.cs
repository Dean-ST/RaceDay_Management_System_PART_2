using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultController(RaceDayDbContext context)
    {
        _context = context;
    }

    // POST: api/results
    [HttpPost]
    public async Task<IActionResult> CreateResult(
        CreateResultRequest request)
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
            .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

        if (enrollment == null)
            return NotFound("Enrollment not found.");

        if (enrollment.Event!.OrganiserId != user.OrganiserId)
            return Forbid();

        if (enrollment.Status != "Confirmed")
            return BadRequest(
                "Only confirmed enrolments can receive results."
            );

        if (request.FinishTime <= TimeSpan.Zero ||
            request.FinishPosition <= 0)
        {
            return BadRequest("Invalid finish time or position.");
        }

        bool resultExists = await _context.Results
            .AnyAsync(r => r.EnrollmentId == request.EnrollmentId);

        if (resultExists)
            return Conflict("A result already exists for this enrollment.");

        var result = new RaceResult
        {
            EnrollmentId = request.EnrollmentId,
            FinishTime = request.FinishTime,
            FinishPosition = request.FinishPosition
        };

        _context.Results.Add(result);

        await _context.SaveChangesAsync();

        return Created(
            $"/api/results/{result.ResultId}",
            new
            {
                result.ResultId,
                result.EnrollmentId,
                result.FinishTime,
                result.FinishPosition
            }
        );
    }

    // GET: api/results/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyResults()
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

        var results = await _context.Results
            .Where(r => r.Enrollment!.ParticipantId == user.ParticipantId)
            .Select(r => new ResultDto(
                r.ResultId,
                r.EnrollmentId,
                r.Enrollment!.Participant!.Name + " " +
                r.Enrollment.Participant.Surname,
                r.Enrollment.Event!.Name,
                r.Enrollment.Category!.Age,
                r.FinishTime,
                r.FinishPosition
            ))
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/results/event/{eventId}
    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventResults(int eventId)
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

        var results = await _context.Results
            .Where(r => r.Enrollment!.EventId == eventId)
            .OrderBy(r => r.FinishPosition)
            .Select(r => new ResultDto(
                r.ResultId,
                r.EnrollmentId,
                r.Enrollment!.Participant!.Name + " " +
                r.Enrollment.Participant.Surname,
                r.Enrollment.Event!.Name,
                r.Enrollment.Category!.Age,
                r.FinishTime,
                r.FinishPosition
            ))
            .ToListAsync();

        return Ok(results);
    }
}