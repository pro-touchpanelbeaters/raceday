# RaceDay API – Setup Guide (Windows)

This guide takes you from a fresh Windows machine to a running RaceDay API with Swagger open in your browser.

There are two ways to do it:

- **Option A – Automatic (recommended):** double-click one file and answer a couple of prompts.
- **Option B – Manual:** do each step yourself. Use this if the script fails or you want to understand each step.

---

## What you need

- Windows 10 or 11
- An internet connection (about 300 MB of downloads the first time)
- Permission to install software on the computer (you will see "Do you want to allow this app to make changes?" prompts – click **Yes**)

---

## Step 1 – Put the project somewhere sensible

1. Copy or unzip the project to a simple folder, for example `C:\Projects\RaceDay-Part2`.
   - Avoid folders synced by **OneDrive**. OneDrive can lock build files and cause random build errors.
2. Open the folder. You should see `RaceDay.sln`, `setup.cmd`, `RaceDay.Api` and `RaceDay.Tests`.

---

## Option A – Automatic setup

1. Double-click **`setup.cmd`**.
2. A window opens and works through these steps. Each one is skipped if it is already done:

   | Step | What happens |
   |---|---|
   | .NET 8 SDK | Installed with `winget` if missing |
   | SQL Server LocalDB | Downloaded and installed if missing, then started |
   | HTTPS certificate | A security dialog asks to trust a "localhost" certificate – click **Yes** |
   | Restore / Build | Downloads NuGet packages and compiles the solution |
   | Unit tests | Runs all unit tests – you should see `Passed!` with 0 failed |

3. When it asks **"Start the API now? [Y/n]"**, press **Enter**.
4. After a few seconds your browser opens Swagger at **http://localhost:5080/swagger**.
5. Leave the window open while you use the API. Press **Ctrl+C** in it to stop the API.

If anything fails, the script prints a red `[X]` message. If it says to open a new window and run setup again, do that. Otherwise, continue with the matching step in Option B.

**Next time** you only need to start the API (see [Starting the API again](#starting-the-api-again)).

---

## Option B – Manual setup

Open **PowerShell** in the project folder (in File Explorer, click the address bar, type `powershell` and press Enter).

### B1. Install the .NET 8 SDK

```powershell
winget install Microsoft.DotNet.SDK.8
```

Or download the **SDK** (not "Runtime") installer from https://dotnet.microsoft.com/download/dotnet/8.0.

Close PowerShell, open a new one, and check it worked:

```powershell
dotnet --list-sdks
```

You should see a line starting with `8.0.`.

### B2. Install SQL Server Express LocalDB

1. Download the SQL Server Express installer: https://www.microsoft.com/sql-server/sql-server-downloads (under **Express**, click **Download now**).
2. Run it and choose **Download Media**.
3. Select **LocalDB** and click **Download**.
4. Open the download folder and run **`SqlLocalDB.msi`**. Accept the licence and finish.
5. Close PowerShell, open a new one, and check it worked:

   ```powershell
   sqllocaldb info
   ```

   You should see `MSSQLLocalDB`.

### B3. Trust the HTTPS development certificate (optional)

```powershell
dotnet dev-certs https --trust
```

Click **Yes** on the security dialog. Without this step, use the `http://` address instead of `https://`.

### B4. Build and test

```powershell
dotnet restore
dotnet build
dotnet test
```

`dotnet test` should end with something like `Passed! - Failed: 0, Passed: 19`.

### B5. Run the API

```powershell
dotnet run --project RaceDay.Api
```

Wait for `Now listening on: http://localhost:5080`, then open **http://localhost:5080/swagger**.

On the very first run the API creates the `RaceDayDb` database and fills it with demo data automatically.

---

## Starting the API again

Open PowerShell in the project folder and run:

```powershell
dotnet run --project RaceDay.Api
```

Then open http://localhost:5080/swagger. Press **Ctrl+C** to stop it.

To use HTTPS instead (needs step B3):

```powershell
dotnet run --project RaceDay.Api --launch-profile https
```

Then open https://localhost:7080/swagger.

---

## Using the API in Swagger

The API uses a login **session**. Once you log in through Swagger, the browser remembers you for the protected endpoints.

1. Expand **POST /api/auth/login** and click **Try it out**.
2. Replace the body with one of the demo accounts and click **Execute**:

   ```json
   { "email": "thabo.organiser@raceday.co.za", "password": "Organiser123!" }
   ```

3. You should get **200** and the user's details.
4. Now try a protected endpoint, for example **GET /api/users/me** or **POST /api/events**.
5. To switch user, call **POST /api/auth/logout** and log in as someone else.

### Demo accounts

| Role | Email | Password |
|---|---|---|
| Organiser | thabo.organiser@raceday.co.za | Organiser123! |
| Organiser | lerato.organiser@raceday.co.za | Organiser123! |
| Participant | participant1@raceday.co.za | Participant123! |
| Participant | participant2@raceday.co.za | Participant123! |

### Expected status codes

| Situation | Code |
|---|---|
| Success (read / update) | 200 |
| Created | 201 |
| Invalid input | 400 |
| Not logged in | 401 |
| Logged in but wrong role / not the owner | 403 |
| Not found | 404 |
| Duplicate (for example enrolling twice) | 409 |

---

## Automatic API check

With the API running, open a **second** PowerShell window in the project folder and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\test-api.ps1
```

It logs in as an organiser and a participant, then creates an event, category, enrolment and result. It checks every response code and deletes the temporary event at the end. You should see **25 passed, 0 failed**. This is handy to show in the demo video.

---

## Viewing the database

The easiest option is **VS Code** with the **SQL Server (mssql)** extension:

1. In VS Code, open Extensions, search for **SQL Server (mssql)** and install it.
2. Click the **SQL Server** icon in the left sidebar, then **Add Connection**, and fill in:
   - **Server name:** `(localdb)\MSSQLLocalDB` (leave Port empty)
   - **Authentication type:** **Windows Authentication**
   - **Trust server certificate:** ticked
   - **Database name:** `RaceDayDb`
3. Click **Connect** and expand **Tables**. Right-click a table and choose **Select Top 1000**.

Alternatively, install **SQL Server Management Studio** (https://aka.ms/ssms) and connect to `(localdb)\MSSQLLocalDB` with Windows Authentication.

---

## Resetting the database

The database is only created when it doesn't exist yet. If you change the entity classes, or want fresh demo data, delete the database and run the API again. Connect to `(localdb)\MSSQLLocalDB` (see above), open a new query against the `master` database, and run:

```sql
ALTER DATABASE RaceDayDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE RaceDayDb;
```

The next `dotnet run` recreates it with the demo data.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `setup.cmd` closes straight away or says scripts are disabled | Right-click `setup.cmd`, choose **Run as administrator**, or run `powershell -ExecutionPolicy Bypass -File .\setup.ps1` |
| `No .NET SDKs were found` | The .NET **runtime** is installed but not the **SDK**. Do step B1, then open a new PowerShell window |
| `sqllocaldb` is not recognized | LocalDB is not installed, or PowerShell was opened before installing. Do step B2, then open a new window |
| `Unable to locate a Local Database Runtime installation` when running the API | Same as above – LocalDB is missing |
| `Couldn't find a project to run` | Use `dotnet run --project RaceDay.Api` from the folder containing `RaceDay.sln` |
| Build error: file is locked / being used by another process | The API is still running. Press **Ctrl+C** in its window, then build again. Also avoid OneDrive folders |
| `Address already in use` / port 5080 in use | Another copy of the API is running. Close it or restart the computer |
| Browser warns the HTTPS site is not secure | Run `dotnet dev-certs https --trust`, or use http://localhost:5080/swagger |
| Login works but the next request returns 401 | Make sure you are calling the API from the **same** Swagger page and address (don't mix `http://` and `https://`) |
| `winget` is not recognized | Install **App Installer** from the Microsoft Store, or use the download links in Option B |
