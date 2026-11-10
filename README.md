# RaceDay Management System

A database-backed race and event management system. The system supports two roles: **Organisers** and **Participants**. Organisers manage their own race events and categories, review enrolments and capture results. Participants browse events, select a category when enrolling, and view their own enrolments and results.


#Database setup

1. Open SQL Server Management Studio (SSMS).
2. Open `Database/SQL/RaceDay_Database.sql`.
3. Execute the script using a SQL Server connection that has permission to create a database.
4. The script creates or uses `RaceDayManagementDB`, creates the seven tables, inserts sample records and runs verification queries.
5. Review the final joined query to confirm that results can be traced through enrolments to participants, events and categories.
Important: The sample `PasswordHash` values are placeholders for the planning database. They are not production credentials. In Part 2, the API must create and verify passwords using a secure password-hashing mechanism.

## Documentation

- **ERD:** `docs/RaceDay_ERD.pdf`
- **API endpoint plan:** `docs/RaceDay_API_Endpoint_Plan.pdf`
- **SQL database script:** `docsDa/RaceDay_Database.sql`


#Roles

#Organiser
- Create and manage their own events.
- Create and manage categories for their events.
- View enrolments for their events.
- Update enrolment status.
- Record results for enrolments in their events.

#Participant
- Browse events and categories.
- Enrol in an event and select a category.
- View their own enrolments and results.
- View and update their own profile.

#Part 1 design corrections and reasons

The Part 1 feedback identified differences between the ERD, SQL script and API endpoint plan. The following corrections were made so that the planning documents and database structure describe the same system.

| Area | Previous issue | Correction made | Reason |
|---|---|---|---|
| ERD – Enrollment | Enrollment did not store the category selected by the participant. | Added `CategoryID` as a foreign key in `ENROLLMENT`. | The selected category is part of the participant's event entry and must be recorded. |
| ERD – Results | Results contained separate participant, event and category references. | Results now contains `EnrollmentID` as its foreign key. | A result is for a specific enrolment. Participant, event and category information can be obtained through that enrolment, avoiding duplicated relationships. |
| ERD – Results key | `CategoryID` was incorrectly marked as a primary key in Results. | `ResultID` remains the primary key; `EnrollmentID` is a foreign key and is unique. | Each result has its own identifier, and an enrolment can have at most one result. |
| SQL – Enrollment | The SQL table did not store the selected category. | Added `CategoryID` and a foreign key to Category. | This makes the SQL table match the corrected ERD and preserves the participant's category choice. |
| SQL – category/event consistency | A category could potentially be selected for a different event. | Added a composite foreign key from `(CategoryID, EventID)` in Enrollment to Category. | This enforces that the selected category belongs to the event being entered. |
| SQL – Results | Results were linked independently to Participant, Event and Category. | Replaced those three foreign keys with `EnrollmentID`. Added a unique constraint on `EnrollmentID`. | This matches the corrected relationship and prevents multiple results for one enrolment. |
| API – login | The login success response planned to return an authentication token. | Changed the plan to session-based authentication with a server-issued session cookie. | The Part 2 guide requires session-based authentication, so the plan must not describe JWT/token authentication. |
| API – enrolment | The request did not clearly capture the participant's selected category. | `POST /api/enrolments` now accepts `EventId` and `CategoryId`; the participant identity comes from the authenticated session. | The API records the selected category while preventing a client from enrolling on behalf of another participant. |
| API – failure handling | Failure response codes were largely missing. | Added relevant `400`, `401`, `403`, `404` and `409` responses to endpoint definitions. | The plan now explains how invalid input, missing authentication, role restrictions, missing records and conflicts are handled. |
| GitHub Actions | The submitted workflow was a default template containing only echo commands. | Replaced it with a workflow that checks required planning files exist and are not empty, and checks for key design terms. | A green workflow now provides an actual repository validation rather than simply running placeholder commands. |

#GitHub Actions validation

The workflow is located at `.github/workflows/ci.yml`. It runs on pushes and pull requests to `main` or `master`, and can also be started manually. It checks that the README, ERD, API endpoint plan and SQL script exist and are not empty. It also checks for key design terms in the SQL and API plan.

A successful green check confirms that these repository files passed the workflow's checks. It does **not** prove that the SQL script has been executed successfully against SQL Server; database execution must still be tested in SSMS.

#Part 2 implementation direction

Part 2 will turn the approved design into a working ASP.NET Core REST API. The implementation will follow the corrected ERD, SQL relationships and endpoint plan. The API will use session-based authentication, role-based access, Entity Framework Core, Swagger and automated tests as required by the Part 2 guide.

Before implementation, confirm that the corrected SQL script runs successfully in SSMS and that the repository contains the files at the paths shown above.
#RaceDay Management System

A database-backed race and event management system. The system supports two roles: **Organisers** and **Participants**. Organisers manage their own race events and categories, review enrolments and capture results. Participants browse events, select a category when enrolling, and view their own enrolments and results.

#Repository structure


#Database setup

1. Open SQL Server Management Studio (SSMS).
2. Open `Database/SQL/RaceDay_Database.sql`.
3. Execute the script using a SQL Server connection that has permission to create a database.
4. The script creates or uses `RaceDayManagementDB`, creates the seven tables, inserts sample records and runs verification queries.
5. Review the final joined query to confirm that results can be traced through enrolments to participants, events and categories.

**Important:** The sample `PasswordHash` values are placeholders for the planning database. They are not production credentials. In Part 2, the API must create and verify passwords using a secure password-hashing mechanism.
Documentation

- ERD: `docs/RaceDay_ERD.pdf`
- API endpoint plan:** `docs/RaceDay_API_Endpoint_Plan.pdf`
- SQL database script:** `docsDa/RaceDay_Database.sql`

The Mermaid ERD source can be previewed in a Markdown editor or a Mermaid-compatible viewer.

#Roles

#Organiser
- Create and manage their own events.
- Create and manage categories for their events.
- View enrolments for their events.
- Update enrolment status.
- Record results for enrolments in their events.

Participant
- Browse events and categories.
- Enrol in an event and select a category.
- View their own enrolments and results.
- View and update their own profile.

Part 1 design corrections and reasons

The Part 1 feedback identified differences between the ERD, SQL script and API endpoint plan. The following corrections were made so that the planning documents and database structure describe the same system.

| Area | Previous issue | Correction made | Reason |
|---|---|---|---|
| ERD – Enrollment | Enrollment did not store the category selected by the participant. | Added `CategoryID` as a foreign key in `ENROLLMENT`. | The selected category is part of the participant's event entry and must be recorded. |
| ERD – Results | Results contained separate participant, event and category references. | Results now contains `EnrollmentID` as its foreign key. | A result is for a specific enrolment. Participant, event and category information can be obtained through that enrolment, avoiding duplicated relationships. |
| ERD – Results key | `CategoryID` was incorrectly marked as a primary key in Results. | `ResultID` remains the primary key; `EnrollmentID` is a foreign key and is unique. | Each result has its own identifier, and an enrolment can have at most one result. |
| SQL – Enrollment | The SQL table did not store the selected category. | Added `CategoryID` and a foreign key to Category. | This makes the SQL table match the corrected ERD and preserves the participant's category choice. |
| SQL – category/event consistency | A category could potentially be selected for a different event. | Added a composite foreign key from `(CategoryID, EventID)` in Enrollment to Category. | This enforces that the selected category belongs to the event being entered. |
| SQL – Results | Results were linked independently to Participant, Event and Category. | Replaced those three foreign keys with `EnrollmentID`. Added a unique constraint on `EnrollmentID`. | This matches the corrected relationship and prevents multiple results for one enrolment. |
| API – login | The login success response planned to return an authentication token. | Changed the plan to session-based authentication with a server-issued session cookie. | The Part 2 guide requires session-based authentication, so the plan must not describe JWT/token authentication. |
| API – enrolment | The request did not clearly capture the participant's selected category. | `POST /api/enrolments` now accepts `EventId` and `CategoryId`; the participant identity comes from the authenticated session. | The API records the selected category while preventing a client from enrolling on behalf of another participant. |
| API – failure handling | Failure response codes were largely missing. | Added relevant `400`, `401`, `403`, `404` and `409` responses to endpoint definitions. | The plan now explains how invalid input, missing authentication, role restrictions, missing records and conflicts are handled. |
| GitHub Actions | The submitted workflow was a default template containing only echo commands. | Replaced it with a workflow that checks required planning files exist and are not empty, and checks for key design terms. | A green workflow now provides an actual repository validation rather than simply running placeholder commands. |

GitHub Actions validation

The workflow is located at `.github/workflows/ci.yml`. It runs on pushes and pull requests to `main` or `master`, and can also be started manually. It checks that the README, ERD, API endpoint plan and SQL script exist and are not empty. It also checks for key design terms in the SQL and API plan.

A successful green check confirms that these repository files passed the workflow's checks. It does **not** prove that the SQL script has been executed successfully against SQL Server; database execution must still be tested in SSMS.

Part 2 implementation direction

Part 2 will turn the approved design into a working ASP.NET Core REST API. The implementation will follow the corrected ERD, SQL relationships and endpoint plan. The API will use session-based authentication, role-based access, Entity Framework Core, Swagger and automated tests as required by the Part 2 guide.

Before implementation, confirm that the corrected SQL script runs successfully in SSMS and that the repository contains the files at the paths shown above.

Part 1 Feedback – Change Report
Summary

The Part 1 feedback showed that the ERD, SQL database script and API endpoint plan were not fully consistent. The repository workflow also did not perform meaningful validation. These items have been corrected before starting Part 2.

Changes completed

ERD updated
   - Added `CategoryID` to `ENROLLMENT`.
   - Changed `RESULTS` to reference `EnrollmentID`.
   - Corrected the primary key and foreign key notation.
   - Updated the relationship so one enrolment can have zero or one result.
SQL script updated
   - Added the selected category foreign key to `Enrollment`.
   - Changed `Results` to use `EnrollmentID`.
   - Added a unique constraint to `Results.EnrollmentID`.
   - Added a composite foreign key to ensure the selected category belongs to the same event.
   - Updated seed data and verification queries to follow the corrected relationships.

3. **API endpoint plan updated**
   - Changed login from token-based authentication to session-based authentication.
   - Added failure response codes to endpoint definitions.
   - Changed participant enrolment to include `EventId` and `CategoryId`.
   - Specified that the participant ID comes from the authenticated session.
   - Changed result creation to accept `EnrollmentId`.

  GitHub Actions workflow updated
   - Replaced the placeholder workflow with repository validation.
   - The workflow checks that the README, ERD, API plan and SQL script exist and are not empty.
   - It also checks for key terms relating to category/enrolment, session authentication and API failure responses.

these changes were necessary

The selected category must be saved because a participant enters a particular event in a particular category. A result should belong to that event entry, rather than maintaining separate participant, event and category links that could contradict one another. Session-based login and documented failure responses make the endpoint plan consistent with the Part 2 requirements. Finally, repository validation ensures the required planning files are actually present instead of allowing a workflow made up of placeholder commands to pass.

Validation still to be completed

- Run the corrected SQL script in SSMS and inspect the created tables and sample data.
- Confirm the corrected ERD, API plan and SQL script are committed at the exact paths checked by GitHub Actions.
- Push the workflow and confirm the Actions run completes successfully.
- Keep meaningful commits for the remaining Part 2 implementation work.


PART 3

# RaceDay Management System

## 1. Project Overview

The RaceDay Management System is a web-based application developed using ASP.NET Core MVC and ASP.NET Core Web API. It allows organisers to manage racing events and participants to browse events and enrol in available categories.

The application uses a REST API to communicate with the database.

## 2. Technologies Used

- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- Microsoft SQL Server
- HTML and CSS
- JavaScript
- Git and GitHub

## 3. User Roles

### Organiser
- Register and log in.
- Manage racing events.
- Create and manage event categories.
- View participant enrolments.
- Update enrolment statuses.

### Participant
- Register and log in.
- Browse available racing events.
- View available event categories.
- Enrol in an event.
- View personal enrolments.

## 4. Solution Structure

The solution contains the following projects:

- **RaceDay.Api:** Handles API requests, authentication and database operations.
- **RaceDay.Contracts:** Contains shared DTOs and request models.
- **RaceDay.Api.Tests:** Contains automated API tests.
- **RaceDay.Mvc:** Provides the web interface and communicates with the API.

## 5. Requirements

To run the application, you need:

- Visual Studio 2026.
- .NET 10 SDK.
- Microsoft SQL Server LocalDB.
- Git.

## 6. Running the Application

### Step 1: Clone the repository

Clone the repository from GitHub and open the solution in Visual Studio.

### Step 2: Configure the database

Ensure that the SQL Server connection string in the API configuration points to your SQL Server instance.

### Step 3: Run the API

Set `RaceDay.Api` as the startup project and run the application.

Swagger can be accessed at:

`http://localhost:5000/swagger`

### Step 4: Configure the MVC application

The MVC application's `appsettings.json` contains the API base address:

```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5000/"
}
```

Update this address if the API runs on a different port.

### Step 5: Run the MVC application

Set `RaceDay.Mvc` as the startup project and run the application.

## 7. Authentication

The application uses session-based authentication.

The MVC application communicates with the API using `HttpClient`. API session cookies are forwarded through a custom HTTP message handler to maintain the authenticated API session.

User roles determine which application features are accessible.

## 8. Main Features

- User registration and login.
- Role-based access.
- Event management.
- Event category management.
- Participant enrolments.
- Enrolment status management.
- MVC interface integrated with the REST API.

## 9. Azure Blob Storage

Azure Blob Storage integration is intended for storing and retrieving event-related images.

Configuration and implementation details will be documented here once completed.

## 10. Docker

Docker containerisation is intended to allow the MVC application to run inside a container.

Docker build and run instructions will be added once containerisation is completed.

## 11. Continuous Integration

The project uses GitHub Actions for automated build and testing workflows.

## 12. Repository

The source code is maintained in GitHub.

Repository:https://github.com/Dean-ST/RaceDay_Management_System_PART_2/edit/PART_3

**Development Branch:** `part-3`

