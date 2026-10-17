namespace RaceDay.Api.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    public int ParticipantId { get; set; }

    public int EventId { get; set; }

    public int CategoryId { get; set; }

    public string Status { get; set; } = "Pending";

    public Participant? Participant { get; set; }

    public RaceEvent? Event { get; set; }

    public Category? Category { get; set; }

    public RaceResult? Result { get; set; }
}