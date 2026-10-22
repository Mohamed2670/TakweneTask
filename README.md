# Takwene Track Management

## Foundation
- .NET SDK 10.0.203 (pinned in global.json).
- Node.js 22.16.0 and npm 10.9.2 used during verification.
- React 19 with TypeScript and Vite; SQL Server integration is planned for Step 2.

## Run backend
From the repository root:
~~~powershell
dotnet restore Takwene.sln
dotnet build Takwene.sln
dotnet run --project backend/Takwene.Api --launch-profile https
~~~
Health endpoint: https://localhost:7270/api/health.
Configure/trust a local HTTPS development certificate before the HTTPS profile is used. Certificate trust has not been verified here.
An HTTP-only smoke check was performed at http://localhost:5037/api/health.

## Run frontend
~~~powershell
cd frontend
npm ci
npm run dev
~~~
Frontend origin: http://localhost:5173.
Production build: npm run build.

Known local limitation: Vite cannot reliably serve source modules from the current E:/firstC#/TawkeneTask path because it contains #. Production builds pass. Use a checkout in a path without # before frontend development; no workspace move has been performed.

## Current scope
Foundation only: no database, migrations, business endpoints, or authentication yet. JWT acquisition and migration instructions will be added when implemented.
See Docs/PROGRESS.md and Docs/HANDOFF.md for the review gate.

## Workspace relocation follow-up
Active workspace: E:\TakweneTask. User approved relocation. Windows locked the original directory, so sources were copied without generated dependencies/build output. Original remains intact and is superseded.
Verified at new path: npm ci; frontend production build; dotnet build with zero warnings/errors; Vite startup; HTTP 200 for HTML, transformed React entry, React and React DOM dependencies. The # path blocker is resolved. Visual browser verification not performed. Verification server stopped.
Await user review before Step 2 persistence. Earlier blocker/environment notes describe the original path and are superseded by this entry.

## SQL Server and migrations (Step 2)
Development defaults target .\SQLEXPRESS, database TakweneTask, using Windows authentication. TrustServerCertificate=True is restricted to the development configuration; use a trusted certificate in production. Set ConnectionStrings__DefaultConnection for other environments.

From E:\TakweneTask:
~~~powershell
dotnet tool restore
dotnet ef database update --project backend/Takwene.Api -- --environment Development
dotnet user-secrets set 'Seed:DemoPassword' '<choose-a-password-of-at-least-12-characters>' --project backend/Takwene.Api
dotnet run --project backend/Takwene.Api --launch-profile http -- --seed-demo-user
sqlcmd -S '.\SQLEXPRESS' -E -C -d TakweneTask -b -i scripts/verify-persistence.sql
~~~
The demo login email is demo@example.com. Seeding creates a missing user only and does not reset an existing user's password. This workspace's initial password was randomly generated and retained in local .NET user secrets; do not commit or share the secret store. Login endpoints are planned for Step 3.
Business sample data is deterministic migration data. The demo user is seeded separately using the framework password hasher. No automatic migrations or demo-user seeding run on normal application startup.
Model annotations and database constraints are present; request DTO validation/normalization and business validation will be implemented in the API milestone.

## Authentication (Step 3)
The API uses access JWTs valid for 10 minutes and opaque refresh tokens valid for an absolute 7-day session. Only refresh-token SHA-256 hashes are stored in SQL Server. Refresh rotates the cookie; replay revokes the whole family. Logout revokes renewal, but an existing access JWT remains valid until expiration.

Development configuration allows exactly http://localhost:5173. Set Cors__AllowedOrigins__0, Jwt__Issuer, Jwt__Audience, and Jwt__SigningKey in other environments. Signing keys must have at least 32 characters of cryptographically generated randomness and must never be committed. Local signing key was generated into .NET user secrets.

To configure a fresh checkout:
~~~powershell
dotnet user-secrets set 'Jwt:SigningKey' '<cryptographically-random-secret-at-least-32-characters>' --project backend/Takwene.Api
# Set Seed:DemoPassword and seed the user as documented above.
dotnet run --project backend/Takwene.Api --launch-profile https
~~~

Request a token through POST https://localhost:7270/api/auth/login, using JSON {"email":"demo@example.com","password":"<your-demo-password>"}. Send Origin: http://localhost:5173 and X-CSRF: 1 headers. The response contains accessToken, expiresAt, and safe user information. Send Authorization: Bearer <accessToken> to GET /api/auth/me.

POST /api/auth/refresh and POST /api/auth/logout require the same Origin and X-CSRF headers plus the refresh cookie. Frontend fetch calls must use credentials: 'include' for login/refresh/logout. The refresh cookie is HttpOnly, Secure, SameSite=None, scoped to /api/auth, with no Domain attribute. SameSite=None supports the current HTTP frontend / HTTPS API development topology; review cookie policy and browser third-party-cookie behavior for final deployment. CORS alone is not our CSRF protection: exact origin validation and the custom request header are required server-side.

React will hold access tokens in memory and refresh on reload in Step 5. It must serialize refresh requests, including across tabs (for example with Web Locks), because strict replay detection intentionally revokes a session when concurrent requests reuse its token. A storage adapter may support sessionStorage later; it is not implemented yet.

Run API integration checks while HTTPS API is running:
~~~powershell
./scripts/verify-auth.ps1
~~~
This local harness reads the configured demo password/signing key from user secrets without printing them. It skips certificate validation only in its test HTTP clients, assumes local SQLEXPRESS/TakweneTask for one expired-refresh case, and creates/revokes test sessions. It does not verify browser cookie handling or certificate trust. Use only on the local development database. Application code never skips HTTPS certificate verification.

Status fields now use TrackStatus and DistributionStatus enums in C#. SQL and JSON retain lowercase names; numeric JSON enum values are rejected.
