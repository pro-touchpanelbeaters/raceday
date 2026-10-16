<#
.SYNOPSIS
    End-to-end check of the running RaceDay API using the seeded demo accounts.

.DESCRIPTION
    Start the API first (dotnet run --project RaceDay.Api), then in a second window run:
    powershell -ExecutionPolicy Bypass -File .\test-api.ps1

    Creates a temporary event, category, enrolment and result, checks every response
    code, and deletes the temporary event at the end.
#>
param([string]$BaseUrl = "http://localhost:5080/api")

$script:passed = 0
$script:failed = 0

function Call($label, $method, $path, $session, $expected, $body = $null) {
    $req = @{ Uri = "$BaseUrl$path"; Method = $method; UseBasicParsing = $true; ContentType = "application/json" }
    if ($session) { $req.WebSession = $session }
    if ($null -ne $body) { $req.Body = ($body | ConvertTo-Json -Compress) }
    try {
        $r = Invoke-WebRequest @req
        $code = [int]$r.StatusCode; $content = $r.Content
    } catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $code = [int]$resp.StatusCode
            $content = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        } else {
            Write-Host ""
            Write-Host "Cannot reach $BaseUrl - is the API running? ($($_.Exception.Message))" -ForegroundColor Red
            exit 1
        }
    }
    if ($code -eq $expected) {
        $script:passed++
        Write-Host ("  PASS  {0,-52} {1}" -f $label, $code) -ForegroundColor Green
    } else {
        $script:failed++
        $short = if ($content -and $content.Length -gt 200) { $content.Substring(0, 200) + "..." } else { $content }
        Write-Host ("  FAIL  {0,-52} got {1}, expected {2}" -f $label, $code, $expected) -ForegroundColor Red
        Write-Host "        $short" -ForegroundColor DarkGray
    }
    if ($content) {
        $trimmed = $content.Trim()
        if ($trimmed.StartsWith("{") -or $trimmed.StartsWith("[")) { return ($trimmed | ConvertFrom-Json) }
    }
}

function Login($email, $password) {
    $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    Call "Login as $email" POST "/auth/login" $s 200 @{ email = $email; password = $password } | Out-Null
    return $s
}

Write-Host "Testing RaceDay API at $BaseUrl"

Write-Host "`nPublic endpoints" -ForegroundColor Cyan
Call "GET all events" GET "/events" $null 200 | Out-Null
Call "GET event 1" GET "/events/1" $null 200 | Out-Null
Call "GET unknown event -> 404" GET "/events/999999" $null 404 | Out-Null
Call "GET categories for event 1" GET "/events/1/categories" $null 200 | Out-Null
Call "GET profile when logged out -> 401" GET "/users/me" $null 401 | Out-Null
Call "Login with wrong password -> 401" POST "/auth/login" (New-Object Microsoft.PowerShell.Commands.WebRequestSession) 401 @{ email = "participant1@raceday.co.za"; password = "wrong-password" } | Out-Null

Write-Host "`nOrganiser" -ForegroundColor Cyan
$org = Login "thabo.organiser@raceday.co.za" "Organiser123!"
Call "GET own profile" GET "/users/me" $org 200 | Out-Null
$ev = Call "Create event" POST "/events" $org 201 @{ name = "API Test 5K"; description = "Temporary event created by test-api.ps1"; eventDate = (Get-Date).AddMonths(2).ToString("s"); location = "Pretoria, Gauteng"; distance = 5; eventType = 0 }
$cat = $null
if ($ev) { $cat = Call "Add category to event" POST "/events/$($ev.eventId)/categories" $org 201 @{ name = "Open"; description = "Open category"; minAge = 18; maxAge = 99; distance = 5 } }
Call "Create event with invalid data -> 400" POST "/events" $org 400 @{ name = ""; location = ""; distance = 0 } | Out-Null
Call "View enrolments for own event" GET "/events/1/enrolments" $org 200 | Out-Null

Write-Host "`nParticipant" -ForegroundColor Cyan
$par = Login "participant2@raceday.co.za" "Participant123!"
Call "GET own profile" GET "/users/me" $par 200 | Out-Null
Call "Participant creates event -> 403" POST "/events" $par 403 @{ name = "x"; location = "x"; distance = 1; eventType = 0; eventDate = (Get-Date).ToString("s") } | Out-Null
$enr = $null
if ($cat) {
    $enr = Call "Enrol in event" POST "/events/$($ev.eventId)/enrolments" $par 201 @{ categoryId = $cat.categoryId }
    Call "Enrol in same event again -> 409" POST "/events/$($ev.eventId)/enrolments" $par 409 @{ categoryId = $cat.categoryId } | Out-Null
}
Call "View my enrolments" GET "/enrolments/my" $par 200 | Out-Null

Write-Host "`nResults" -ForegroundColor Cyan
$res = $null
if ($enr) { $res = Call "Organiser records result" POST "/enrolments/$($enr.enrolmentId)/result" $org 201 @{ finishTime = "00:24:31"; finishingPosition = 1 } }
if ($res) { Call "Organiser publishes result" PUT "/results/$($res.resultId)" $org 200 @{ finishTime = "00:24:31"; finishingPosition = 1; publish = $true } | Out-Null }
if ($ev) { Call "Public event results" GET "/events/$($ev.eventId)/results" $null 200 | Out-Null }
Call "Participant views my results" GET "/results/my" $par 200 | Out-Null

Write-Host "`nLogout and clean-up" -ForegroundColor Cyan
Call "Participant logs out" POST "/auth/logout" $par 200 | Out-Null
Call "GET profile after logout -> 401" GET "/users/me" $par 401 | Out-Null
if ($ev) { Call "Delete temporary event" DELETE "/events/$($ev.eventId)" $org 200 | Out-Null }

Write-Host ""
$colour = if ($script:failed -eq 0) { "Green" } else { "Red" }
Write-Host "$($script:passed) passed, $($script:failed) failed" -ForegroundColor $colour
if ($script:failed -gt 0) { exit 1 }
