using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/profile/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var user = await _context.Users
            .Include(u => u.Organiser)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound("User profile not found.");
        }

        string firstName = user.Organiser?.Name
            ?? user.Participant?.Name
            ?? string.Empty;

        string lastName = user.Organiser?.Surname
            ?? user.Participant?.Surname
            ?? string.Empty;

        string phoneNumber = user.Organiser?.PhoneNumber
            ?? user.Participant?.PhoneNumber
            ?? string.Empty;

        int? age = user.Participant?.Age;

        var profile = new ProfileDto(
            user.UserId,
            firstName,
            lastName,
            user.Email,
            user.UserType,
            phoneNumber,
            age
        );

        return Ok(profile);
    }

    // PUT: api/profile/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateProfileRequest request)
    {
        int? userId = HttpContext.Session.GetInt32(SessionKeys.UserId);

        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest("Name and phone number are required.");
        }

        var user = await _context.Users
            .Include(u => u.Organiser)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound("User profile not found.");
        }

        if (user.UserType == "Organiser" && user.Organiser != null)
        {
            user.Organiser.Name = request.FirstName.Trim();
            user.Organiser.Surname = request.LastName.Trim();
            user.Organiser.PhoneNumber = request.PhoneNumber.Trim();
        }
        else if (user.UserType == "Participant" &&
                 user.Participant != null)
        {
            if (request.Age == null || request.Age < 1)
            {
                return BadRequest("A valid age is required.");
            }

            user.Participant.Name = request.FirstName.Trim();
            user.Participant.Surname = request.LastName.Trim();
            user.Participant.PhoneNumber = request.PhoneNumber.Trim();
            user.Participant.Age = request.Age.Value;
        }
        else
        {
            return BadRequest("User profile information is incomplete.");
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Profile updated successfully."
        });
    }
}