namespace RaceDay.Api.Models;

public class RaceResult
{
    public int ResultId { get; set; }

    public int EnrollmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int FinishPosition { get; set; }

    public Enrollment? Enrollment { get; set; }
}