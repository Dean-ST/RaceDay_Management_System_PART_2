namespace RaceDay.Api.Models;

public class Organiser
{
    public int OrganiserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>();

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}