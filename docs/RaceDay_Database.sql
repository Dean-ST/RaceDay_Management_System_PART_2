/*
    RaceDay Management System
    Part 1 - Corrected Microsoft SQL Server database script

    Seven entities:
      Organisers, Participants, [USER], Events, Category, Enrollment, Results

    Key corrections:
      1. Enrollment stores the selected CategoryID.
      2. Results references Enrollment through EnrollmentID only.
      3. A result is limited to one per enrolment.
      4. A composite foreign key ensures the selected category belongs
         to the same event as the enrolment.

*/

IF DB_ID(N'RaceDayManagementDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayManagementDB;
END;
GO

USE RaceDayManagementDB;
GO

/* Drop existing tables in dependency order so this script can be rerun. */
DROP TABLE IF EXISTS dbo.Results;
DROP TABLE IF EXISTS dbo.Enrollment;
DROP TABLE IF EXISTS dbo.Category;
DROP TABLE IF EXISTS dbo.Events;
DROP TABLE IF EXISTS dbo.[USER];
DROP TABLE IF EXISTS dbo.Participants;
DROP TABLE IF EXISTS dbo.Organisers;
GO

CREATE TABLE dbo.Organisers
(
    OrganiserID INT IDENTITY(1,1) CONSTRAINT PK_Organisers PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Surname NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_Organisers_Email UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL
);
GO

CREATE TABLE dbo.Participants
(
    ParticipantID INT IDENTITY(1,1) CONSTRAINT PK_Participants PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Surname NVARCHAR(50) NOT NULL,
    Age INT NOT NULL CONSTRAINT CK_Participants_Age CHECK (Age > 0 AND Age < 120),
    Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_Participants_Email UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL
);
GO

-- login account linked to exactly one profile type
CREATE TABLE dbo.[USER]
(
    UserID INT IDENTITY(1,1) CONSTRAINT PK_User PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_User_Email UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    UserType NVARCHAR(20) NOT NULL
        CONSTRAINT CK_User_UserType CHECK (UserType IN ('Organiser', 'Participant')),
    OrganiserID INT NULL,
    ParticipantID INT NULL,

    CONSTRAINT FK_User_Organiser FOREIGN KEY (OrganiserID)
        REFERENCES dbo.Organisers(OrganiserID),
    CONSTRAINT FK_User_Participant FOREIGN KEY (ParticipantID)
        REFERENCES dbo.Participants(ParticipantID),
    CONSTRAINT CK_User_ProfileMatch CHECK
    (
        (UserType = 'Organiser' AND OrganiserID IS NOT NULL AND ParticipantID IS NULL)
        OR
        (UserType = 'Participant' AND ParticipantID IS NOT NULL AND OrganiserID IS NULL)
    )
);
GO

CREATE TABLE dbo.Events
(
    EventID INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    [Date] DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL
        CONSTRAINT CK_Events_Distance CHECK (Distance > 0),
    EventType NVARCHAR(50) NOT NULL,
    OrganiserID INT NOT NULL,

    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID)
        REFERENCES dbo.Organisers(OrganiserID)
);
GO

CREATE TABLE dbo.Category
(
    CategoryID INT IDENTITY(1,1) CONSTRAINT PK_Category PRIMARY KEY,
    Age INT NOT NULL CONSTRAINT CK_Category_Age CHECK (Age > 0 AND Age < 120),
    EventID INT NOT NULL,

    CONSTRAINT FK_Category_Event FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    --Supports the composite FK from Enrollment, ensuring event/category match.
    CONSTRAINT UQ_Category_Category_Event UNIQUE (CategoryID, EventID)
);
GO

--records the participant's selected category
CREATE TABLE dbo.Enrollment
(
    EnrollmentID INT IDENTITY(1,1) CONSTRAINT PK_Enrollment PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT FK_Enrollment_Participant FOREIGN KEY (ParticipantID)
        REFERENCES dbo.Participants(ParticipantID),
    CONSTRAINT FK_Enrollment_Event FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),
    CONSTRAINT FK_Enrollment_Category_Event FOREIGN KEY (CategoryID, EventID)
        REFERENCES dbo.Category(CategoryID, EventID),

    CONSTRAINT UQ_Enrollment_Participant_Event UNIQUE (ParticipantID, EventID)
);
GO

-- every result belongs to an enrolment
CREATE TABLE dbo.Results
(
    ResultID INT IDENTITY(1,1) CONSTRAINT PK_Results PRIMARY KEY,
    EnrollmentID INT NOT NULL,
    FinishTime TIME(0) NOT NULL,
    FinishPosition INT NOT NULL
        CONSTRAINT CK_Results_FinishPosition CHECK (FinishPosition > 0),

    CONSTRAINT FK_Results_Enrollment FOREIGN KEY (EnrollmentID)
        REFERENCES dbo.Enrollment(EnrollmentID),
    CONSTRAINT UQ_Results_Enrollment UNIQUE (EnrollmentID)
);
GO

INSERT INTO dbo.Organisers (Name, Surname, Email, PhoneNumber)
VALUES
(N'Thabo', N'Mokoena', N'thabo.mokoena@raceday.co.za', N'0825551001'),
(N'Lerato', N'Dlamini', N'lerato.dlamini@raceday.co.za', N'0835551002');

INSERT INTO dbo.Participants (Name, Surname, Age, Email, PhoneNumber)
VALUES
(N'Sipho', N'Nkosi', 24, N'sipho.nkosi@email.com', N'0845552001'),
(N'Anele', N'Mthembu', 29, N'anele.mthembu@email.com', N'0855552002');

/*
  Demonstration-only password hash placeholders.
  These are NOT production-ready login credentials. Part 2 must create
  password hashes with the application's secure password-hashing mechanism.
*/
INSERT INTO dbo.[USER] (Email, PasswordHash, UserType, OrganiserID, ParticipantID)
VALUES
(N'thabo.mokoena@raceday.co.za', N'PART2_HASH_REQUIRED_ORGANISER_1', N'Organiser', 1, NULL),
(N'lerato.dlamini@raceday.co.za', N'PART2_HASH_REQUIRED_ORGANISER_2', N'Organiser', 2, NULL),
(N'sipho.nkosi@email.com', N'PART2_HASH_REQUIRED_PARTICIPANT_1', N'Participant', NULL, 1),
(N'anele.mthembu@email.com', N'PART2_HASH_REQUIRED_PARTICIPANT_2', N'Participant', NULL, 2);

INSERT INTO dbo.Events
    (Name, Description, [Date], Location, Distance, EventType, OrganiserID)
VALUES
(N'Bloemfontein City Run',
 N'A city road race suitable for recreational and competitive runners.',
 '2026-10-10', N'Bloemfontein, Free State', 10.00, N'Road Race', 1),
(N'Mangaung Half Marathon',
 N'A 21 kilometre endurance race through Mangaung.',
 '2026-11-15', N'Mangaung, Free State', 21.10, N'Half Marathon', 2),
(N'Free State Fun Run',
 N'A community-focused race for runners of different experience levels.',
 '2026-12-05', N'Bloemfontein, Free State', 5.00, N'Fun Run', 1);

--two age categories for each event
INSERT INTO dbo.Category (Age, EventID)
VALUES
(18, 1), (30, 1),
(18, 2), (30, 2),
(18, 3), (30, 3);


  --SEED DATA: each enrolment explicitly stores the selected category.
  --Category IDs 1/3/5 belong to events 1/2/3 respectively.

INSERT INTO dbo.Enrollment (ParticipantID, EventID, CategoryID)
VALUES
(1, 1, 1),
(2, 1, 1),
(1, 2, 3),
(2, 2, 3),
(1, 3, 5),
(2, 3, 5);

--results refer to enrolment IDs, not participant/event/category separately
INSERT INTO dbo.Results (EnrollmentID, FinishTime, FinishPosition)
VALUES
(1, '00:52:34', 1),
(2, '00:57:48', 2),
(3, '01:58:21', 1),
(4, '02:05:42', 2),
(5, '00:26:15', 1),
(6, '00:29:37', 2);
GO

--CHECKING TABLE CONTENTS
SELECT * FROM dbo.Organisers;
SELECT * FROM dbo.Participants;
SELECT * FROM dbo.[USER];
SELECT * FROM dbo.Events;
SELECT * FROM dbo.Category;
SELECT * FROM dbo.Enrollment;
SELECT * FROM dbo.Results;

--VERIFY RESULTS THROUGH THE ENROLMENT RELATIONSHIP
SELECT
    r.ResultID,
    r.EnrollmentID,
    p.Name AS ParticipantName,
    p.Surname AS ParticipantSurname,
    e.Name AS EventName,
    c.Age AS CategoryAge,
    r.FinishTime,
    r.FinishPosition
FROM dbo.Results AS r
INNER JOIN dbo.Enrollment AS en ON r.EnrollmentID = en.EnrollmentID
INNER JOIN dbo.Participants AS p ON en.ParticipantID = p.ParticipantID
INNER JOIN dbo.Events AS e ON en.EventID = e.EventID
INNER JOIN dbo.Category AS c ON en.CategoryID = c.CategoryID;
GO
