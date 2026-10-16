<#
.SYNOPSIS
    One-time setup for the RaceDay API on Windows.

.DESCRIPTION
    1. Installs the .NET 8 SDK (if missing)
    2. Installs SQL Server Express LocalDB (if missing) and starts it
    3. Trusts the ASP.NET Core HTTPS development certificate
    4. Restores, builds and tests the solution
    5. Optionally starts the API and opens Swagger

    Safe to run more than once - steps that are already done are skipped.

.EXAMPLE
    Double-click setup.cmd, or from PowerShell in this folder:
    powershell -ExecutionPolicy Bypass -File .\setup.ps1
#>
param(
    [switch]$SkipTests,
    [switch]$SkipCertificate,
    [switch]$NoRun
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

function Step($text) { Write-Host ""; Write-Host "==> $text" -ForegroundColor Cyan }
function Ok($text)   { Write-Host "    [OK] $text" -ForegroundColor Green }
function Warn($text) { Write-Host "    [!]  $text" -ForegroundColor Yellow }
function Fail($text) {
    Write-Host ""
    Write-Host "    [X]  $text" -ForegroundColor Red
    Write-Host "    See SETUP.md for the manual steps." -ForegroundColor Red
    exit 1
}

function Refresh-Path {
    $machine = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $user = [Environment]::GetEnvironmentVariable("Path", "User")
    $env:Path = "$machine;$user"
}

function Test-Command($name) { return [bool](Get-Command $name -ErrorAction SilentlyContinue) }

function Find-SqlLocalDb {
    if (Test-Command "sqllocaldb") { return (Get-Command sqllocaldb).Source }
    $exe = Get-ChildItem "C:\Program Files\Microsoft SQL Server" -Recurse -Filter SqlLocalDB.exe -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending | Select-Object -First 1
    if ($exe) { return $exe.FullName }
    return $null
}

function Test-DotNet8Sdk {
    if (-not (Test-Command "dotnet")) { return $false }
    $sdks = & dotnet --list-sdks 2>$null
    return [bool]($sdks | Where-Object { $_ -match '^8\.' })
}

Write-Host "RaceDay API - setup" -ForegroundColor White
Write-Host "Project folder: $PSScriptRoot"

# --------------------------------------------------------------------------
Step "Checking winget (Windows Package Manager)"
if (Test-Command "winget") {
    Ok "winget is available"
} else {
    Warn "winget was not found. Install 'App Installer' from the Microsoft Store, or install the prerequisites manually (see SETUP.md)."
}

# --------------------------------------------------------------------------
Step "Checking .NET 8 SDK"
if (Test-DotNet8Sdk) {
    Ok ".NET 8 SDK is installed"
} else {
    if (-not (Test-Command "winget")) { Fail ".NET 8 SDK is missing and winget is not available to install it." }
    Write-Host "    Installing .NET 8 SDK (you may see a Windows permission prompt)..."
    winget install --id Microsoft.DotNet.SDK.8 --exact --silent --accept-package-agreements --accept-source-agreements
    Refresh-Path
    if (-not (Test-DotNet8Sdk)) { Fail ".NET 8 SDK installation did not complete. Close this window, open a new one and run setup again." }
    Ok ".NET 8 SDK installed"
}
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

# --------------------------------------------------------------------------
Step "Checking SQL Server Express LocalDB"
$sqlLocalDb = Find-SqlLocalDb
if ($sqlLocalDb) {
    Ok "LocalDB is installed"
} else {
    if (-not (Test-Command "winget")) { Fail "LocalDB is missing and winget is not available to download it." }

    $work = Join-Path $env:TEMP "RaceDay-LocalDB"
    if (Test-Path $work) { Remove-Item $work -Recurse -Force }
    New-Item -ItemType Directory -Path $work | Out-Null

    Write-Host "    Downloading the SQL Server Express installer..."
    winget download --id Microsoft.SQLServer.2025.Express --exact --download-directory $work --accept-package-agreements --accept-source-agreements | Out-Null
    $ssei = Get-ChildItem $work -Filter *.exe | Select-Object -First 1
    if (-not $ssei) { Fail "Could not download the SQL Server Express installer." }

    Write-Host "    Downloading the LocalDB package (about 50 MB)..."
    $media = Join-Path $work "media"
    Start-Process -FilePath $ssei.FullName -ArgumentList "/ACTION=Download", "/MEDIAPATH=`"$media`"", "/MEDIATYPE=LocalDB", "/QUIET" -Wait
    $msi = Get-ChildItem $media -Recurse -Filter SqlLocalDB.msi -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $msi) { Fail "Could not download SqlLocalDB.msi." }

    Write-Host "    Installing LocalDB (accept the Windows permission prompt)..."
    $p = Start-Process msiexec.exe -ArgumentList "/i `"$($msi.FullName)`" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES" -Verb RunAs -Wait -PassThru
    if ($p.ExitCode -ne 0 -and $p.ExitCode -ne 3010) { Fail "LocalDB installer exited with code $($p.ExitCode)." }

    Refresh-Path
    $sqlLocalDb = Find-SqlLocalDb
    if (-not $sqlLocalDb) { Fail "LocalDB installation did not complete." }
    Ok "LocalDB installed"
}

$instances = & $sqlLocalDb info
if (-not ($instances -contains "MSSQLLocalDB")) {
    & $sqlLocalDb create MSSQLLocalDB | Out-Null
}
& $sqlLocalDb start MSSQLLocalDB | Out-Null
Ok "LocalDB instance (localdb)\MSSQLLocalDB is running"

# --------------------------------------------------------------------------
Step "Trusting the HTTPS development certificate"
if ($SkipCertificate) {
    Warn "Skipped (-SkipCertificate). Use the http:// address."
} else {
    & dotnet dev-certs https --check --trust *> $null
    if ($LASTEXITCODE -eq 0) {
        Ok "Development certificate already trusted"
    } else {
        Write-Host "    A security dialog may appear - click 'Yes' to trust the localhost certificate."
        & dotnet dev-certs https --trust *> $null
        if ($LASTEXITCODE -eq 0) { Ok "Development certificate trusted" }
        else { Warn "Certificate was not trusted. The http:// address will still work." }
    }
}

# --------------------------------------------------------------------------
Step "Restoring NuGet packages"
& dotnet restore RaceDay.sln
if ($LASTEXITCODE -ne 0) { Fail "dotnet restore failed. Check your internet connection." }
Ok "Packages restored"

Step "Building the solution"
& dotnet build RaceDay.sln --no-restore
if ($LASTEXITCODE -ne 0) { Fail "Build failed. If the API is already running, stop it (Ctrl+C) and run setup again." }
Ok "Build succeeded"

if (-not $SkipTests) {
    Step "Running unit tests"
    $testOutput = & dotnet test RaceDay.sln --no-build 2>&1 | ForEach-Object { Write-Host "    $_"; "$_" }
    $summary = $testOutput | Where-Object { $_ -match 'Total:\s+\d+' } | Select-Object -Last 1
    if ($LASTEXITCODE -ne 0) { Warn "Some unit tests failed - see the output above." }
    elseif (-not $summary) { Warn "No unit tests were run - check that RaceDay.Tests builds." }
    else { Ok "All unit tests passed" }
}

# --------------------------------------------------------------------------
Write-Host ""
Write-Host "Setup complete." -ForegroundColor Green
Write-Host ""
Write-Host "  Start the API:  dotnet run --project RaceDay.Api"
Write-Host "  Swagger:        http://localhost:5080/swagger"
Write-Host "  Test the API:   powershell -ExecutionPolicy Bypass -File .\test-api.ps1   (while the API is running)"
Write-Host ""

if (-not $NoRun) {
    $answer = Read-Host "Start the API now? [Y/n]"
    if ($answer -eq "" -or $answer -match '^[Yy]') {
        Write-Host "Starting the API - press Ctrl+C to stop it." -ForegroundColor Cyan
        Start-Job -ScriptBlock { Start-Sleep -Seconds 12; Start-Process "http://localhost:5080/swagger" } | Out-Null
        & dotnet run --project RaceDay.Api --no-build --launch-profile http
    }
}
