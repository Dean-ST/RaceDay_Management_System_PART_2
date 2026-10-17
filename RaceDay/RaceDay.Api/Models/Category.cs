namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryId { get; set; }

    public int Age { get; set; }

    public int EventId { get; set; }

    public RaceEvent? Event { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}