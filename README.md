# RaceDay - Part 2 RESTful API

RaceDay is an ASP.NET Core Web API for managing running, walking and cycling events. Part 2 implements the Part 1 API plan using C#, EF Core and SQL Server, with server-side session authentication, role-based authorization, Swagger documentation, unit tests and GitHub Actions CI/CD.

## Roles

### Organiser
- Register and log in.
- View and update own profile.
- Create, update and delete own events.
- Create, update and delete categories for owned events.
- View enrolments for owned events.
- Capture and publish participant results for owned events.

### Participant
- Register and log in.
- View and update own profile.
- Browse events and categories.
- Enrol in an event by selecting a category.
- View and cancel own enrolments.
- View own published results.

## Technology

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core Code First
- SQL Server
- Server-side ASP.NET Core Session
- BCrypt password hashing
- Swagger / OpenAPI
- xUnit + Moq / EF Core InMemory for unit tests
- GitHub Actions

## Project structure

```text
RaceDay.sln
├── RaceDay.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Entities/
│   ├── Middleware/
│   ├── Services/
│   └── Program.cs
├── RaceDay.Tests/
└── .github/workflows/part2-ci.yml
```

## Database setup

1. Install SQL Server / SQL Server Express or LocalDB.
2. Create the RaceDay database using the approved Part 1 SQL script, or allow the API's EF Core `EnsureCreated` startup path to create the schema for a development database.
3. Update `RaceDay.Api/appsettings.json` if your SQL Server instance differs.

The EF Core model is intended to match the Part 1 relational design. Before submission, compare the generated database schema against the approved Part 1 ERD and SQL script.

## Run locally

**New machine?** Follow [SETUP.md](SETUP.md), or double-click `setup.cmd` to install the prerequisites (.NET 8 SDK, SQL Server LocalDB), build, test and start the API automatically.

From the solution directory:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project RaceDay.Api
```

Open Swagger at:

```text
https://localhost:7080/swagger
```

## Demo accounts

The development seeder creates:

| Role | Email | Password |
|---|---|---|
| Organiser | thabo.organiser@raceday.co.za | Organiser123! |
| Organiser | lerato.organiser@raceday.co.za | Organiser123! |
| Participant | participant1@raceday.co.za | Participant123! |
| Participant | participant2@raceday.co.za | Participant123! |

Change these credentials before any real deployment.

## Authentication design

RaceDay uses server-side session management. A successful login stores `UserId`, `Role` and `UserEmail` in the ASP.NET Core session. `SessionAuthenticationMiddleware` converts the session values into claims for the current request, allowing ASP.NET Core `[Authorize]` and `[Authorize(Roles = "...")]` to enforce protected routes. `SessionAuthenticationHandler` is registered as the authentication scheme so that unauthenticated requests return `401 Unauthorized` and requests with the wrong role return `403 Forbidden`.

Passwords are never stored in their original form. BCrypt hashes are stored in `PasswordHash`.

## Endpoint summary

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/logout` | Logged in |
| GET | `/api/users/me` | Logged in |
| PUT | `/api/users/me` | Logged in |
| GET | `/api/events` | Public |
| GET | `/api/events/{id}` | Public |
| POST | `/api/events` | Organiser |
| PUT | `/api/events/{id}` | Organiser owner |
| DELETE | `/api/events/{id}` | Organiser owner |
| GET | `/api/events/{eventId}/categories` | Public |
| GET | `/api/categories/{id}` | Public |
| POST | `/api/events/{eventId}/categories` | Organiser owner |
| PUT | `/api/categories/{id}` | Organiser owner |
| DELETE | `/api/categories/{id}` | Organiser owner |
| POST | `/api/events/{eventId}/enrolments` | Participant |
| GET | `/api/enrolments/my` | Participant |
| GET | `/api/events/{eventId}/enrolments` | Organiser owner |
| DELETE | `/api/enrolments/{enrolmentId}` | Participant owner |
| POST | `/api/enrolments/{enrolmentId}/result` | Organiser owner |
| GET | `/api/results/my` | Participant |
| GET | `/api/events/{eventId}/results` | Public |
| PUT | `/api/results/{id}` | Organiser owner |

## Unit testing

The test suite covers:
- Registration success.
- Password hashing.
- Duplicate registration failure.
- Login success and failure.
- Organiser event creation.
- Event ownership enforcement.
- Event deletion.
- Participant enrolment.
- Duplicate enrolment rejection.
- Enrolment cancellation.
- Controller role metadata for Organiser/Participant protected operations.

Run:

```bash
dotnet test RaceDay.sln
```

## GitHub Actions

The workflow in `.github/workflows/part2-ci.yml` restores, builds and tests the solution on every push and pull request.

### CI screenshot

Replace this section with a screenshot of the successful green GitHub Actions build before submission:

`docs/images/part2-ci-green.png`

## Video

Replace the placeholder with your unlisted YouTube video link:

`YOUTUBE_LINK_TO_BE_ADDED`

The video should demonstrate the running API, authentication, role restrictions, Swagger endpoints and unit tests, as required by the POE.
