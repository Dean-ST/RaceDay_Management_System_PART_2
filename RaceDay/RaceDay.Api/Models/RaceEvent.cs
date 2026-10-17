namespace RaceDay.Api.Models;

public class RaceEvent
{
    public int EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Location { get; set; } = string.Empty;

    public decimal Distance { get; set; }

    public string EventType { get; set; } = string.Empty;

    public int OrganiserId { get; set; }

    public Organiser? Organiser { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}