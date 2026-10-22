# Takwene Track Management

Track Management API for the Takwene full-stack assignment. Backend: one .NET Web API project using controllers, services, repositories, EF Core, and SQL Server. Frontend: React/TypeScript login, track library with status filtering, and track details with DSP statuses.

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
dotnet user-secrets set 'Seed:DemoPassword' '<choose-6-to-256-character-development-password>' --project backend/Takwene.Api
$jwtKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
@{ 'Jwt:SigningKey' = $jwtKey } | ConvertTo-Json | dotnet user-secrets set --project backend/Takwene.Api
dotnet run --project backend/Takwene.Api --launch-profile http -- --seed-demo-user
dotnet run --project backend/Takwene.Api --launch-profile https
~~~
The seed command is development-only and creates a missing demo@example.com user. It does not reset an existing password. The locally selected demo password is in .NET user secrets. To explicitly replace an existing demo password, stop the API, update Seed:DemoPassword, then run `dotnet run --project backend/Takwene.Api --launch-profile http -- --reset-demo-password`. This Development-only command hashes the replacement and revokes existing refresh sessions; normal startup never resets credentials. Development seed passwords accept 6–256 characters; this is not a public registration policy. Business data is seeded deterministically through migrations: 3 artists, 9 tracks, 3 DSPs, and 8 distributions. Normal startup does not migrate or seed automatically.

API: https://localhost:7270; HTTP profile: http://localhost:5037. Configure/trust a local HTTPS certificate for browser use. The local certificate is trusted and actual browser login/refresh was verified. GET /api/health is a basic liveness check, not a database readiness check.

## Authentication
POST /api/auth/login with Content-Type: application/json, Origin: http://localhost:5173, X-CSRF: 1, and body:
~~~json
{ "email": "demo@example.com", "password": "<your-demo-password>" }
~~~
The response returns accessToken, expiresAt, and safe user details; it sets the refresh cookie. Send Authorization: Bearer <accessToken> to protected routes. GET /api/auth/me verifies current user information.

Access JWTs last 10 minutes and are not stored in SQL Server. Refresh tokens are opaque random values, hashed with SHA-256 in SQL, and rotated with an absolute 7-day session lifetime. Reuse revokes the whole family. Logout prevents renewal; an already-issued access JWT remains valid until expiry.

POST /api/auth/refresh and POST /api/auth/logout need the refresh cookie, allowed Origin, and X-CSRF: 1. Auth POSTs require HTTPS. Browser login/refresh/logout calls must use credentials: 'include'. Cookie: __Secure-Takwene.Refresh; HttpOnly; Secure; SameSite=None; path=/api/auth; no Domain attribute. SameSite=None supports the current HTTP frontend/HTTPS API topology; review same-site hosting and browser cookie behavior during frontend integration/deployment.

Only http://localhost:5173 is allowlisted in development. Other environments must configure Cors:AllowedOrigins, Jwt:Issuer, Jwt:Audience, Jwt:SigningKey and the database connection. CORS uses exact origins; server-side origin and custom-header checks provide CSRF protection for auth POSTs.

React keeps access tokens in memory behind a get/set/clear storage adapter and restores sessions on reload. Web Locks serialize login, refresh and logout across same-origin tabs. BroadcastChannel sends sign-in/sign-out signals without credentials. Modern browser support for Web Locks and a secure context (localhost qualifies) is required. No sessionStorage adapter is implemented.

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

## Frontend
~~~powershell
cd frontend
npm ci
npm run dev
~~~
Frontend: http://localhost:5173. Build: npm run build. Tests: npm test (22 checks). Copy .env.example to .env.local only to override VITE_API_URL; its default is https://localhost:7270/api. Restart Vite after changing environment configuration.

Routes: /login, /tracks, /tracks/:id. The two catalog views require a restored staff session in the UI. Status filtering is sent to the API and preserved in the URL; title/artist search filters the returned small catalog locally. Loading, empty, not-found and retry states are included. Login trims the email and preserves the password exactly. No editing forms are in the assignment UI scope.

Production hosting must serve index.html for SPA routes and configure the API URL plus exact CORS origin. Revisit cookie topology and browser policy for the deployed origins.

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
The local PowerShell harnesses read app credentials from user secrets without printing them and skip certificate validation in test clients only. They assume the local SQL instance/database for test expiry/cleanup. Track fixtures are removed; revoked refresh-test records remain. Application code never disables certificate validation. Browser behavior is tested separately: local login, secure-cookie session restore, simultaneous tab reloads, cross-tab sign-in/out, status/details/search, mobile overflow and API-outage recovery passed in the in-app browser. This is not a full browser compatibility or accessibility audit.

Logging/rate-limit hardening remains Step 6. See Docs/PROGRESS.md, Docs/HANDOFF.md and milestone reports. Local Git commits are created per completed step; the user handles pushing. Early history was reconstructed transparently in Docs/GIT-HISTORY.md.

## DTO mapping
AutoMapper 16.2.0 provides the seven one-way mappings in backend/Takwene.Api/Profiles: ArtistProfile, TrackProfile, AuthProfile. MappingRegistration registers them; the API validates configuration at startup. Services call IMapper for data conversion while preserving explicit validation, initial status, transitions, timestamps, transactions and security operations. No reverse entity mapping or SQL projection changes are introduced.

AutoMapper reports a missing-license warning allowing development/testing; production licensing is not configured. The warning is not suppressed. If you have an applicable key, configure AutoMapper:LicenseKey through .NET user secrets or AutoMapper__LicenseKey in the server environment. Do not commit keys. See https://docs.automapper.io/en/latest/License-configuration.html and Docs/MAPPING-REVIEW.md.

## Access-token generator
Services/TokenGenerator implements ITokenGenerator.GenerateAccessToken(User) using SecurityTokenDescriptor and JwtSecurityTokenHandler. It returns an AccessTokenResult with the signed token and exact UTC expiry. It uses validated JwtOptions, HS256, sub/email/jti, and explicit issued-at/not-before/expiry; lifetime remains 10 minutes. AuthService composes the public response and handles session orchestration. Refresh tokens remain opaque random values with the existing SQL hash/rotation policy.
The email claim is visible in the signed JWT payload; no password/hash/refresh token/role/branch data is included. The auth verification script checks claims, exact expiry, 600-second lifetime and new token IDs on refresh. See Docs/TOKEN-GENERATOR-REVIEW.md.

## Refresh-token generator
Services/RefreshTokenGenerator implements IRefreshTokenGenerator.GenerateRefreshToken() and HashToken(string). It creates a 32-byte cryptographically random opaque value, returning its 64-character hexadecimal token and SHA256 hash in RefreshTokenResult. AuthService persists only the hash and handles created/expiry/family metadata, rotation and revocation; only the cookie receives the raw token. Initial session expiry remains seven days and rotation preserves the original expiry. No refresh JWT is issued.
The auth harness checks the generator format and persisted hash, cookie/session expiry agreement, absolute lifetime preservation and replacement links using only test-created sessions. See Docs/REFRESH-TOKEN-GENERATOR-REVIEW.md.
