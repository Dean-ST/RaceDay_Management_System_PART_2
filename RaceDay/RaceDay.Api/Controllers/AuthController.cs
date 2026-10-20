using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    private readonly PasswordHasher<AppUser> _passwordHasher;

    public AuthController(RaceDayDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<AppUser>();
    }

    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest("All required fields must be completed.");
        }

        string role = request.Role.Trim();

        if (role != "Organiser" && role != "Participant")
        {
            return BadRequest("Role must be Organiser or Participant.");
        }

        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict("An account with this email already exists.");
        }

        if (request.Password.Length < 8)
        {
            return BadRequest("Password must contain at least 8 characters.");
        }

        var user = new AppUser
        {
            Email = email,
            UserType = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        if (role == "Organiser")
        {
            var organiser = new Organiser
            {
                Name = request.FirstName.Trim(),
                Surname = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber
            };

            user.Organiser = organiser;
        }
        else
        {
            if (request.Age == null || request.Age < 1)
            {
                return BadRequest("A valid age is required for participants.");
            }

            var participant = new Participant
            {
                Name = request.FirstName.Trim(),
                Surname = request.LastName.Trim(),
                Age = request.Age.Value,
                Email = email,
                PhoneNumber = request.PhoneNumber
            };

            user.Participant = participant;
        }

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Created(
            "/api/profile/me",
            new AuthResponse(
                user.UserId,
                user.Email,
                user.UserType,
                "Account registered successfully."
            )
        );
    }

    // LOGIN
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        HttpContext.Session.SetInt32(
            SessionKeys.UserId,
            user.UserId
        );

        HttpContext.Session.SetString(
            SessionKeys.Role,
            user.UserType
        );

        return Ok(new AuthResponse(
            user.UserId,
            user.Email,
            user.UserType,
            "Login successful."
        ));
    }

    // LOGOUT
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return Ok(new
        {
            Message = "Logged out successfully."
        });
    }
}