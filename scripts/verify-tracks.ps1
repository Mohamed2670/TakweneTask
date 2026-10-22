param([string]$BaseUrl='https://localhost:7270')
$ErrorActionPreference='Stop'
[xml]$project=Get-Content (Join-Path $PSScriptRoot '../backend/Takwene.Api/Takwene.Api.csproj')
$secretId=$project.Project.PropertyGroup.UserSecretsId | Where-Object { $_ }
$secrets=Get-Content (Join-Path $env:APPDATA "Microsoft/UserSecrets/$secretId/secrets.json") -Raw | ConvertFrom-Json
$session=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
function Call-Api($Method,$Path,$Body=$null,$Headers=@{}) {
    $session.Headers.Clear() # PowerShell sessions retain custom headers; isolate each request.
    $params=@{ Method=$Method; Uri="$BaseUrl$Path"; Headers=$Headers; WebSession=$session; SkipCertificateCheck=$true; SkipHttpErrorCheck=$true }
    if ($null -ne $Body) { $params.Body=($Body | ConvertTo-Json -Depth 8 -Compress); $params.ContentType='application/json' }
    Invoke-WebRequest @params
}
function Expect($Response,[int]$Code,[string]$Label) {
    if ([int]$Response.StatusCode -ne $Code) { throw "$Label expected $Code; got $($Response.StatusCode)." }
    Write-Output "PASS: $Label"
}
$login=Call-Api POST '/api/auth/login' @{ email='demo@example.com'; password=$secrets.'Seed:DemoPassword' } @{ Origin='http://localhost:5173'; 'X-CSRF'='1' }
if ($login.StatusCode -ne 200) { throw 'Demo login failed.' }
$auth=@{ Authorization='Bearer '+($login.Content | ConvertFrom-Json).accessToken }
$artistId=$null
$tag=[Guid]::NewGuid().ToString('N')
try {
    Expect (Call-Api GET '/api/artists') 200 'public artist list'
    Expect (Call-Api GET '/api/tracks') 200 'public track list'
    Expect (Call-Api GET '/api/tracks/3') 200 'seeded track detail'
    $seedDetail=(Call-Api GET '/api/tracks/3').Content | ConvertFrom-Json
    if (-not $seedDetail.artistName -or $seedDetail.distributions[0].status -ne 'live' -or -not $seedDetail.distributions[0].dspName) { throw 'Artist/distribution detail projection failed.' }
    Write-Output 'PASS: artist and DSP status details'
    $artistBody=@{ name="  Verification $tag  "; email="  $tag@example.com  "; country='  Egypt  ' }
    Expect (Call-Api POST '/api/artists' $artistBody) 401 'artist creation protected'
    Expect (Call-Api POST '/api/artists' @{ name=' '; email='bad'; country=' ' } $auth) 400 'artist field validation'
    Expect (Call-Api POST '/api/artists' @{ name=('x'*151); email='valid@example.com'; country='Egypt' } $auth) 400 'artist length limit'
    $artistResponse=Call-Api POST '/api/artists' $artistBody $auth
    Expect $artistResponse 201 'artist creation'
    $artist=$artistResponse.Content | ConvertFrom-Json
    $artistId=$artist.id
    if ($artist.name -ne "Verification $tag" -or $artist.country -ne 'Egypt' -or $artist.email -ne "$tag@example.com") { throw 'Artist trimming failed.' }
    Write-Output 'PASS: artist normalization'
    $digits=[Security.Cryptography.RandomNumberGenerator]::GetInt32(10000000).ToString('D7')
    $canonical="EGTST$digits"
    $body=@{ title='  Verification Track  '; artistId=$artistId; isrc=" eg-tst-$digits "; releaseDate='2030-01-01'; genre='  Jazz  ' }
    Expect (Call-Api POST '/api/tracks' $body) 401 'track creation protected'
    $bad=$body.Clone(); $bad.artistId=2147483647
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'unknown artist rejected'
    $bad=$body.Clone(); $bad.title=' '
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'blank title rejected'
    $bad=$body.Clone(); $bad.isrc='bad'
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'invalid ISRC rejected'
    $bad=$body.Clone(); $bad.releaseDate='2030-02-30'
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'invalid calendar date rejected'
    $bad=$body.Clone(); $bad.Remove('releaseDate')
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'missing date rejected'
    $bad=$body.Clone(); $bad.status='distributed'
    Expect (Call-Api POST '/api/tracks' $bad $auth) 400 'creation status overposting rejected'
    $created=Call-Api POST '/api/tracks' $body $auth
    Expect $created 201 'track creation'
    $track=$created.Content | ConvertFrom-Json
    $id=$track.id
    if ($track.title -ne 'Verification Track' -or $track.genre -ne 'Jazz' -or $track.isrc -ne $canonical -or $track.status -ne 'draft') { throw 'Track normalization/default status failed.' }
    if (-not $created.Headers.Location) { throw 'Created track location missing.' }
    Write-Output 'PASS: track trimming ISRC normalization and draft default'
    Expect (Call-Api POST '/api/tracks' $body $auth) 409 'duplicate ISRC conflict'
    $filtered=Call-Api GET "/api/tracks?artistId=$artistId&genre=%20Jazz%20&status=draft"
    Expect $filtered 200 'combined filters'
    $items=@($filtered.Content | ConvertFrom-Json)
    if ($items.Count -ne 1 -or $items[0].id -ne $id) { throw 'Combined filters returned wrong records.' }
    if ($items[0].artistName -ne $artist.name) { throw 'Track-list artist-name mapping failed.' }
    Write-Output 'PASS: mapped list artist name'
    Expect (Call-Api GET '/api/tracks?status=%20%20') 200 'blank status filter treated as absent'
    Expect (Call-Api GET '/api/tracks?status=0') 400 'numeric status filter rejected'
    Expect (Call-Api GET '/api/tracks?status=invalid') 400 'invalid status filter rejected'
    Expect (Call-Api GET '/api/tracks?artistId=-1') 400 'invalid artist filter rejected'
    Expect (Call-Api GET '/api/tracks/2147483647') 404 'missing track'
    Expect (Call-Api GET '/api/tracks/0') 400 'nonpositive track ID'
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1) }) 401 'distribution protected'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='submitted' }) 401 'status edit protected'
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@() } $auth) 400 'empty DSP selection rejected'
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1,1) } $auth) 400 'duplicate request DSP IDs rejected'
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1,2147483647) } $auth) 400 'unknown DSP rejects entire request'
    $unchanged=(Call-Api GET "/api/tracks/$id").Content | ConvertFrom-Json
    if ($unchanged.status -ne 'draft' -or @($unchanged.distributions).Count -ne 0) { throw 'Invalid distribution partially wrote data.' }
    Write-Output 'PASS: rejected distribution is atomic'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='distributed' } $auth) 409 'skipped transition rejected'
    $submitted=Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1) } $auth
    Expect $submitted 200 'distribution submission'
    if ($submitted.Content -notmatch '"submittedAt"\s*:\s*"[^"]+Z"') { throw 'Mapped distribution timestamp is not UTC.' }
    Write-Output 'PASS: mapped distribution UTC timestamp'
    $distribution=$submitted.Content | ConvertFrom-Json
    if ($distribution.status -ne 'submitted' -or $distribution.distributions[0].status -ne 'pending' -or $distribution.distributions[0].dspName -ne 'Spotify') { throw 'Submission state incorrect.' }
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1,2) } $auth) 409 'existing DSP rejects mixed request'
    $unchanged=(Call-Api GET "/api/tracks/$id").Content | ConvertFrom-Json
    if (@($unchanged.distributions).Count -ne 1) { throw 'Duplicate request partially wrote data.' }
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(2) } $auth) 200 'additional DSP submission'
    # Two submissions to the same new DSP: database lock plus uniqueness must allow only one.
    $handler=[Net.Http.HttpClientHandler]::new()
    $handler.ServerCertificateCustomValidationCallback=[Net.Http.HttpClientHandler]::DangerousAcceptAnyServerCertificateValidator
    $client=[Net.Http.HttpClient]::new($handler)
    try {
        $requests=1..2 | ForEach-Object {
            $request=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::Post,"$BaseUrl/api/tracks/$id/distribute")
            $request.Headers.Add('Authorization',$auth.Authorization)
            $request.Content=[Net.Http.StringContent]::new('{"dspIds":[3]}',[Text.Encoding]::UTF8,'application/json')
            $request
        }
        $tasks=@($client.SendAsync($requests[0]),$client.SendAsync($requests[1]))
        $responses=@($tasks[0].GetAwaiter().GetResult(),$tasks[1].GetAwaiter().GetResult())
        $codes=@($responses | ForEach-Object { [int]$_.StatusCode } | Sort-Object)
        if (($codes -join ',') -ne '200,409') { throw 'Concurrent distribution did not produce one success and one conflict.' }
        foreach ($response in $responses) { $response.Dispose() }
        foreach ($request in $requests) { $request.Dispose() }
    } finally { $client.Dispose() }
    Write-Output 'PASS: concurrent duplicate distribution serialized'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='invalid' } $auth) 400 'unknown status rejected'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status=2 } $auth) 400 'numeric JSON enum rejected'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='2' } $auth) 400 'numeric enum string rejected'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{} $auth) 400 'missing status rejected'
    $finished=Call-Api PATCH "/api/tracks/$id/status" @{ status='distributed' } $auth
    Expect $finished 200 'manual distributed status with pending DSPs'
    $finishedData=$finished.Content | ConvertFrom-Json
    if (($finishedData.distributions.dspId -join ',') -ne '1,2,3') { throw 'Mapped distribution ordering failed.' }
    if ($finishedData.artistName -ne $artist.name) { throw 'Track-detail artist-name mapping failed.' }
    Write-Output 'PASS: mapped detail artist name and distribution ordering'
    if (@($finishedData.distributions | Where-Object status -ne 'pending').Count -ne 0) { throw 'Track status changed DSP statuses.' }
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='distributed' } $auth) 200 'same status is idempotent'
    Expect (Call-Api PATCH "/api/tracks/$id/status" @{ status='draft' } $auth) 409 'backward transition rejected'
    Expect (Call-Api POST "/api/tracks/$id/distribute" @{ dspIds=@(1) } $auth) 409 'distributed track resubmission rejected'
    Expect (Call-Api POST '/api/tracks/2147483647/distribute' @{ dspIds=@(1) } $auth) 404 'distribution missing track'
    Expect (Call-Api PATCH '/api/tracks/2147483647/status' @{ status='submitted' } $auth) 404 'status missing track'
    $manualBody=$body.Clone(); $manualBody.isrc='EGMAN'+$digits
    $manual=Call-Api POST '/api/tracks' $manualBody $auth
    Expect $manual 201 'manual-only test track'
    $manualId=($manual.Content | ConvertFrom-Json).id
    Expect (Call-Api PATCH "/api/tracks/$manualId/status" @{ status='submitted' } $auth) 200 'manual submitted status'
    $manualFinal=Call-Api PATCH "/api/tracks/$manualId/status" @{ status='distributed' } $auth
    Expect $manualFinal 200 'manual distributed without DSP records'
    if (@(($manualFinal.Content | ConvertFrom-Json).distributions).Count -ne 0) { throw 'Manual status unexpectedly created distributions.' }
    $empty=Call-Api GET "/api/tracks?artistId=$artistId&status=draft"
    if (@($empty.Content | ConvertFrom-Json).Count -ne 0) { throw 'Empty filter should return an empty array.' }
    Write-Output 'PASS: empty filter result'
} finally {
    if ($artistId) {
        # Remove only fixtures belonging to this script-created artist; preserve seed/user data.
        $cleanup="SET XACT_ABORT ON; BEGIN TRAN; DELETE FROM TrackDistributions WHERE TrackId IN (SELECT Id FROM Tracks WHERE ArtistId=$artistId); DELETE FROM Tracks WHERE ArtistId=$artistId; DELETE FROM Artists WHERE Id=$artistId; COMMIT;"
        $null=& sqlcmd -S '.\SQLEXPRESS' -E -C -d TakweneTask -b -Q $cleanup
        if ($LASTEXITCODE -ne 0) { throw 'Test-fixture cleanup failed.' }
    }
    $null=Call-Api POST '/api/auth/logout' $null @{ Origin='http://localhost:5173'; 'X-CSRF'='1' }
}
Write-Output 'Track API integration checks passed; business fixtures removed.'
