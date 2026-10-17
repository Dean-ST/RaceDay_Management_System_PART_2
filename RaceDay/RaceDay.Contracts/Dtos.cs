namespace RaceDay.Contracts;

// AUTHENTICATION

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    int? Age,
    string PhoneNumber
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    int UserId,
    string Email,
    string Role,
    string Message
);


// USER PROFILE

public record ProfileDto(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string PhoneNumber,
    int? Age
);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    int? Age
);


// EVENTS

public record CreateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal Distance,
    string EventType
);

public record UpdateEventRequest(
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal Distance,
    string EventType
);

public record EventDto(
    int EventId,
    string Name,
    string Description,
    DateTime Date,
    string Location,
    decimal Distance,
    string EventType,
    int OrganiserId
);


// CATEGORIES

public record CreateCategoryRequest(
    int Age
);

public record CategoryDto(
    int CategoryId,
    int Age,
    int EventId
);


// ENROLMENTS

public record CreateEnrolmentRequest(
    int EventId,
    int CategoryId
);

public record UpdateEnrolmentStatusRequest(
    string Status
);

public record EnrolmentDto(
    int EnrollmentId,
    int ParticipantId,
    string ParticipantName,
    int EventId,
    string EventName,
    int CategoryId,
    int CategoryAge,
    string Status
);


// RESULTS

public record CreateResultRequest(
    int EnrollmentId,
    TimeSpan FinishTime,
    int FinishPosition
);

public record ResultDto(
    int ResultId,
    int EnrollmentId,
    string ParticipantName,
    string EventName,
    int CategoryAge,
    TimeSpan FinishTime,
    int FinishPosition
);
