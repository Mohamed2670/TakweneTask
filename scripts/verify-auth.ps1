param([string]$BaseUrl = 'https://localhost:7270')
$ErrorActionPreference = 'Stop'
# Local development only. Certificate trust is skipped by this harness, not by application code.
[xml]$project = Get-Content (Join-Path $PSScriptRoot '../backend/Takwene.Api/Takwene.Api.csproj')
$secretId = $project.Project.PropertyGroup.UserSecretsId | Where-Object { $_ }
$secrets = Get-Content (Join-Path $env:APPDATA "Microsoft/UserSecrets/$secretId/secrets.json") -Raw | ConvertFrom-Json
$password = $secrets.'Seed:DemoPassword'
if (-not $password) { throw 'Local demo password is not configured.' }
$headers = @{ Origin='http://localhost:5173'; 'X-CSRF'='1' }
$cookieName = '__Secure-Takwene.Refresh'
function Call-Api($Method, $Path, $Session, $Headers=$headers, $Body=$null) {
    $params = @{ Method=$Method; Uri="$BaseUrl$Path"; Headers=$Headers; SkipCertificateCheck=$true; SkipHttpErrorCheck=$true }
    if ($null -ne $Session) { $params.WebSession=$Session }
    if ($null -ne $Body) { $params.Body=($Body | ConvertTo-Json -Compress); $params.ContentType='application/json' }
    Invoke-WebRequest @params
}
function Expect($Response, [int]$Code, [string]$Label) {
    if ([int]$Response.StatusCode -ne $Code) { throw "$Label failed: expected $Code, received $($Response.StatusCode)" }
    Write-Output "PASS: $Label"
}
function Session-WithToken([string]$Token) {
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $cookie = [Net.Cookie]::new($cookieName,$Token,'/api/auth','localhost'); $cookie.Secure=$true
    $session.Cookies.Add($cookie)
    return $session
}
function New-Login {
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $response = Call-Api POST '/api/auth/login' $session $headers @{ email='  DEMO@example.com  '; password=$password }
    if ($response.StatusCode -ne 200) { throw 'Demo login failed.' }
    return @{ Session=$session; Response=$response; Data=($response.Content | ConvertFrom-Json) }
}
Expect (Call-Api GET '/api/auth/me' $null) 401 'anonymous protected request'
Expect (Call-Api POST '/api/auth/login' $null $headers @{ email='demo@example.com'; password='incorrect' }) 401 'incorrect password'
Expect (Call-Api POST '/api/auth/login' $null $headers @{ email='not-an-email'; password='x' }) 400 'input validation'
Expect (Call-Api POST '/api/auth/refresh' $null @{ Origin='https://attacker.example'; 'X-CSRF'='1' }) 403 'untrusted origin rejected'
Expect (Call-Api POST '/api/auth/refresh' $null @{ Origin='http://localhost:5173' }) 403 'missing CSRF header rejected'
Expect (Call-Api POST '/api/auth/refresh' $null) 401 'missing refresh cookie'
$preflight = Call-Api OPTIONS '/api/auth/refresh' $null @{ Origin='http://localhost:5173'; 'Access-Control-Request-Method'='POST'; 'Access-Control-Request-Headers'='x-csrf,content-type' }
if ($preflight.Headers['Access-Control-Allow-Origin'] -ne 'http://localhost:5173' -or $preflight.Headers['Access-Control-Allow-Credentials'] -ne 'true') { throw 'CORS allowlist preflight failed.' }
Write-Output 'PASS: allowed credentialed CORS preflight'
$blocked = Call-Api OPTIONS '/api/auth/refresh' $null @{ Origin='https://attacker.example'; 'Access-Control-Request-Method'='POST' }
if ($blocked.Headers['Access-Control-Allow-Origin']) { throw 'Untrusted CORS origin accepted.' }
Write-Output 'PASS: untrusted CORS origin omitted'
$login = New-Login
$cookieHeader = $login.Response.Headers['Set-Cookie'] -join ';'
foreach ($attribute in @('httponly','secure','samesite=none','path=/api/auth')) { if ($cookieHeader.ToLowerInvariant() -notlike "*$attribute*") { throw 'Missing refresh cookie security attribute.' } }
if ($login.Data.PSObject.Properties.Name -contains 'refreshToken') { throw 'Refresh token leaked in response body.' }
$publicUserFields=@($login.Data.user.PSObject.Properties.Name | Sort-Object)
if (($publicUserFields -join ',') -ne 'email,id,name') { throw 'User mapping exposed unexpected fields.' }
Write-Output 'PASS: mapped user contains only public fields'
Write-Output 'PASS: login, trimmed email, cookie attributes, safe response'
$access = $login.Data.accessToken
Expect (Call-Api GET '/api/auth/me' $null @{ Authorization="Bearer $access" }) 200 'valid JWT'
Expect (Call-Api GET '/api/auth/me' $null @{ Authorization='Bearer invalid.token.value' }) 401 'invalid JWT'
$oldToken = $login.Session.Cookies.GetCookies([uri]("$BaseUrl/api/auth/refresh"))[$cookieName].Value
$refresh = Call-Api POST '/api/auth/refresh' $login.Session
Expect $refresh 200 'refresh rotation'
$newToken = $login.Session.Cookies.GetCookies([uri]("$BaseUrl/api/auth/refresh"))[$cookieName].Value
if ($oldToken -eq $newToken) { throw 'Refresh token was not rotated.' }
Expect (Call-Api POST '/api/auth/refresh' (Session-WithToken $oldToken)) 401 'replayed refresh rejected'
Expect (Call-Api POST '/api/auth/refresh' (Session-WithToken $newToken)) 401 'replayed family revoked'
$logoutLogin=New-Login
$logoutToken=$logoutLogin.Session.Cookies.GetCookies([uri]("$BaseUrl/api/auth/refresh"))[$cookieName].Value
Expect (Call-Api POST '/api/auth/logout' $logoutLogin.Session) 204 'logout'
Expect (Call-Api POST '/api/auth/refresh' (Session-WithToken $logoutToken)) 401 'logout revokes refresh session'
# Sign intentionally invalid JWT claims with the local test key; never print tokens or secrets.
function Base64Url([byte[]]$Value) { [Convert]::ToBase64String($Value).TrimEnd('=').Replace('+','-').Replace('/','_') }
function Test-Jwt([string]$Issuer,[string]$Audience,[long]$Expiry) {
    $now=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $head=Base64Url ([Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))
    $payload=@{ sub='1'; iss=$Issuer; aud=$Audience; iat=$now-1200; nbf=$now-1200; exp=$Expiry } | ConvertTo-Json -Compress
    $unsigned=$head+'.'+(Base64Url ([Text.Encoding]::UTF8.GetBytes($payload)))
    $hmac=[Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($secrets.'Jwt:SigningKey'))
    try { return $unsigned+'.'+(Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($unsigned)))) } finally { $hmac.Dispose() }
}
$now=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
foreach ($case in @(@('Takwene.Api','Takwene.Frontend',($now-60),'expired JWT'),@('other','Takwene.Frontend',($now+600),'wrong issuer'),@('Takwene.Api','other',($now+600),'wrong audience'))) {
    $token=Test-Jwt $case[0] $case[1] $case[2]
    Expect (Call-Api GET '/api/auth/me' $null @{ Authorization="Bearer $token" }) 401 $case[3]
}



$expiredLogin=New-Login
$expiredRaw=$expiredLogin.Session.Cookies.GetCookies([uri]("$BaseUrl/api/auth/refresh"))[$cookieName].Value
$expiredHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($expiredRaw)))
# Only this test-created token is changed; values satisfy the database date constraint.
$query="UPDATE RefreshTokens SET CreatedAt=DATEADD(day,-8,SYSUTCDATETIME()), ExpiresAt=DATEADD(second,-1,SYSUTCDATETIME()) WHERE TokenHash='$expiredHash';"
$null = & sqlcmd -S '.\SQLEXPRESS' -E -C -d TakweneTask -b -Q $query
if ($LASTEXITCODE -ne 0) { throw 'Failed to prepare expired refresh case.' }
Expect (Call-Api POST '/api/auth/refresh' $expiredLogin.Session) 401 'expired refresh token'
$null=Call-Api POST '/api/auth/logout' (Session-WithToken $expiredRaw)
$parallelLogin=New-Login
$parallelRaw=$parallelLogin.Session.Cookies.GetCookies([uri]("$BaseUrl/api/auth/refresh"))[$cookieName].Value
$handler=[Net.Http.HttpClientHandler]::new()
$handler.UseCookies=$false
$handler.ServerCertificateCustomValidationCallback=[Net.Http.HttpClientHandler]::DangerousAcceptAnyServerCertificateValidator
$client=[Net.Http.HttpClient]::new($handler)
try {
    $requests=1..2 | ForEach-Object {
        $request=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::Post,"$BaseUrl/api/auth/refresh")
        $request.Headers.Add('Origin','http://localhost:5173'); $request.Headers.Add('X-CSRF','1')
        $request.Headers.Add('Cookie',"$cookieName=$parallelRaw")
        $request
    }
    $tasks=@($client.SendAsync($requests[0]),$client.SendAsync($requests[1]))
    $responses=@($tasks[0].GetAwaiter().GetResult(),$tasks[1].GetAwaiter().GetResult())
    $codes=@($responses | ForEach-Object { [int]$_.StatusCode } | Sort-Object)
    if (($codes -join ',') -ne '200,401') { throw 'Concurrent refresh did not yield one success and one rejection.' }
    Write-Output 'PASS: concurrent refresh serialized'
    $winner=$responses | Where-Object { [int]$_.StatusCode -eq 200 }
    $winnerCookie=@($winner.Headers.GetValues('Set-Cookie'))[0]
    $winnerRaw=([regex]::Match($winnerCookie,'^[^=]+=([^;]+)')).Groups[1].Value
    Expect (Call-Api POST '/api/auth/refresh' (Session-WithToken $winnerRaw)) 401 'concurrent reuse revokes winning session'
    foreach ($response in $responses) { $response.Dispose() }
    foreach ($request in $requests) { $request.Dispose() }
} finally { $client.Dispose() }
Write-Output 'Authentication integration checks passed. Test refresh sessions remain revoked in the local database.'
