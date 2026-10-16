# Part 2 Implementation Notes

## Specification alignment

The implementation follows the RaceDay Part 1 resource plan: Authentication, User Profile, Events, Categories, Event Enrolments and Results.

## Session authentication

The POE requires server-side session management. The API therefore does not use JWT as its primary authentication mechanism. Login stores the authenticated user's ID and role in the ASP.NET Core server-side session. Middleware creates claims from the session for authorization on subsequent requests.

## Part 3 preparation

The API separates business logic into services so that Azure Blob Storage can be added to the API in Part 3 without moving file-handling logic into the MVC application. The MVC application must continue to use the API for data and file operations.

## Important submission check

Before submitting Part 2, compare this implementation against the final approved Part 1 endpoint plan and ERD. The POE explicitly states that the Part 1 plan is the API specification and that the Part 2 schema must match the Part 1 ERD and SQL script.
