# Part 2 Test Matrix

| Area | Success scenario | Failure scenario |
|---|---|---|
| Registration | New Organiser/Participant account is created | Duplicate email is rejected |
| Login | Correct credentials create a session | Incorrect credentials return 401 |
| Password | Password is stored as BCrypt hash | Plain-text password is never stored |
| Events | Organiser creates/deletes own event | Non-owner cannot update/delete |
| Categories | Organiser manages categories for own event | Invalid age range is rejected |
| Enrolments | Participant enrols in event category | Duplicate enrolment is rejected |
| Enrolments | Participant cancels own enrolment | Other participant cannot cancel it |
| Results | Organiser records result for own event | Other organiser cannot modify result |
| Authorization | Organiser-only action accepts Organiser | Participant is rejected from Organiser-only action |
| Authorization | Participant-only action accepts Participant | Organiser is rejected from Participant-only action |
