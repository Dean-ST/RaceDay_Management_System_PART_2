namespace RaceDay.Api.Models;

public class AppUser
{
    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string UserType { get; set; } = string.Empty;

    public int? OrganiserId { get; set; }

    public int? ParticipantId { get; set; }

    public Organiser? Organiser { get; set; }

    public Participant? Participant { get; set; }
}