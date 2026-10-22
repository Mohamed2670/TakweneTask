# Takwene Track Management

Track Management API for the Takwene full-stack assignment. Backend: one .NET Web API project using controllers, services, repositories, EF Core, and SQL Server. Frontend: React/TypeScript foundation; login and track views are the next milestone.

## Prerequisites
Verified locally: .NET SDK 10.0.203 (global.json), Node 22.16.0, npm 10.9.2, SQL Server Express instance .\SQLEXPRESS. Use a workspace path without #; the active workspace is E:\TakweneTask.

## Backend setup
Run from the project root:
~~~powershell
dotnet restore Takwene.sln
dotnet tool restore
dotnet ef database update --project backend/Takwene.Api -- --environment Development
~~~
Development settings use Windows authentication to .\SQLEXPRESS, database TakweneTask. To use another database, configure ConnectionStrings:DefaultConnection through user secrets or ConnectionStrings__DefaultConnection. The development-only connection trusts the local SQL certificate; production must use a trusted certificate.

Configure local secrets (never commit their values):
~~~powershell
dotnet user-secrets set 'Seed:DemoPassword' '<choose-12-to-256-character-password>' --project backend/Takwene.Api
$jwtKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
@{ 'Jwt:SigningKey' = $jwtKey } | ConvertTo-Json | dotnet user-secrets set --project backend/Takwene.Api
dotnet run --project backend/Takwene.Api --launch-profile http -- --seed-demo-user
dotnet run --project backend/Takwene.Api --launch-profile https
~~~
The seed command is development-only and creates a missing demo@example.com user. It does not reset an existing password. This workspace's initial demo password is already in local .NET user secrets. Business data is seeded deterministically through migrations: 3 artists, 9 tracks, 3 DSPs, and 8 distributions. Normal startup does not migrate or seed automatically.

API: https://localhost:7270; HTTP profile: http://localhost:5037. Configure/trust a local HTTPS certificate for browser use. A valid certificate exists locally, but browser trust has not yet been verified. GET /api/health is a basic liveness check, not a database readiness check.

## Authentication
POST /api/auth/login with Content-Type: application/json, Origin: http://localhost:5173, X-CSRF: 1, and body:
~~~json
{ "email": "demo@example.com", "password": "<your-demo-password>" }
~~~
The response returns accessToken, expiresAt, and safe user details; it sets the refresh cookie. Send Authorization: Bearer <accessToken> to protected routes. GET /api/auth/me verifies current user information.

Access JWTs last 10 minutes and are not stored in SQL Server. Refresh tokens are opaque random values, hashed with SHA-256 in SQL, and rotated with an absolute 7-day session lifetime. Reuse revokes the whole family. Logout prevents renewal; an already-issued access JWT remains valid until expiry.

POST /api/auth/refresh and POST /api/auth/logout need the refresh cookie, allowed Origin, and X-CSRF: 1. Auth POSTs require HTTPS. Browser login/refresh/logout calls must use credentials: 'include'. Cookie: __Secure-Takwene.Refresh; HttpOnly; Secure; SameSite=None; path=/api/auth; no Domain attribute. SameSite=None supports the current HTTP frontend/HTTPS API topology; review same-site hosting and browser cookie behavior during frontend integration/deployment.

Only http://localhost:5173 is allowlisted in development. Other environments must configure Cors:AllowedOrigins, Jwt:Issuer, Jwt:Audience, Jwt:SigningKey and the database connection. CORS uses exact origins; server-side origin and custom-header checks provide CSRF protection for auth POSTs.

React will keep access tokens in memory and refresh on reload. Refresh coordination across tabs is required because strict replay detection also revokes sessions when simultaneous refreshes reuse an old token. The token-storage extension point is planned for Step 5; sessionStorage is not currently used.

## Artist and track API
Reads are public; all writes require a bearer access token.

| Method | Route | Behavior |
| --- | --- | --- |
| POST | /api/artists | Create artist; 201 |
| GET | /api/artists | List artists |
| POST | /api/tracks | Create draft track; 201 with Location |
| GET | /api/tracks?artistId=&genre=&status= | Combine optional filters |
| GET | /api/tracks/{id} | Artist information and DSP distribution details |
| POST | /api/tracks/{id}/distribute | Submit to one or more DSPs atomically |
| PATCH | /api/tracks/{id}/status | Manually advance track status |

Example artist: {"name":"Layla Noor","email":"layla@example.com","country":"Egypt"}.
Example track:
~~~json
{ "title": "First Light", "artistId": 1, "isrc": "EGTKW2600010", "releaseDate": "2030-01-01", "genre": "Arabic Pop" }
~~~
Distribution body: {"dspIds":[1,2]}. Status body: {"status":"submitted"}. Status names are lowercase strings, never numeric values.

Names/emails/country/title/genre/ISRC are trimmed. ISRC is normalized to uppercase without display hyphens, then checked for its 12-character format and uniqueness. Required fields, lengths, dates, IDs, enums and referenced records are validated. Passwords are not trimmed. Creation DTOs reject unknown fields, including status overrides. Model validation also runs in services before persistence.

Tracks start draft. Distribution creates pending rows and moves a draft to submitted in one transaction. Duplicate requested DSP IDs return 400; existing distributions return 409 for the whole request. Missing DSPs reject the whole request. Track status advances draft -> submitted -> distributed; repeated status is idempotent, backwards/skipped transitions return 409. PATCH never changes DSP statuses and can mark a submitted track distributed without live DSP records. Distributed tracks cannot be resubmitted. Real DSP integration/status callbacks are outside scope.

Invalid inputs return 400 with ProblemDetails/field errors; unauthenticated writes return 401; missing track resources return 404; uniqueness/state conflicts return 409. Unknown referenced artist/DSP IDs return 400. Empty optional text filters act as absent filters. Genre equality follows SQL Server collation; lists are ordered by ID. Pagination is not implemented for this assignment's small dataset.

## Frontend foundation
~~~powershell
cd frontend
npm ci
npm run dev
~~~
Frontend: http://localhost:5173. Build: npm run build. The project foundation works; actual login/track UI remains Step 5.

## Verification
~~~powershell
dotnet build Takwene.sln
dotnet ef migrations has-pending-model-changes --project backend/Takwene.Api -- --environment Development
dotnet list backend/Takwene.Api package --vulnerable --include-transitive
# With HTTPS API running:
./scripts/verify-auth.ps1
./scripts/verify-tracks.ps1
sqlcmd -S '.\SQLEXPRESS' -E -C -d TakweneTask -b -i scripts/verify-persistence.sql
~~~
The local PowerShell harnesses read app credentials from user secrets without printing them and skip certificate validation in test clients only. They assume the local SQL instance/database for test expiry/cleanup. Track fixtures are removed; revoked refresh-test records remain. Application code never disables certificate validation. Browser rendering/cookie behavior and certificate trust are not verified by these scripts.

Logging/rate-limit hardening remains Step 6. See Docs/PROGRESS.md, Docs/HANDOFF.md and milestone reports. Local Git commits are created per completed step; the user handles pushing. Early history was reconstructed transparently in Docs/GIT-HISTORY.md.
