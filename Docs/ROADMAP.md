# Roadmap

Steps 1–2 completed and reviewed. Step 3 implemented and awaiting review; Steps 4–7 planned. Each ends with verification, Docs updates, a report, and a user review gate.

| Step | Deliverable | Acceptance checks |
| --- | --- | --- |
| 0 — Planning | Working agreement, schema, architecture, behavior | User reviews Docs and resolves open decisions |
| 1 — Foundation | One backend project/solution, React app, configuration and dependency registration | Backend and frontend build; actual tool versions and commands documented; no secrets tracked |
| 2 — Persistence | EF models, relationship configurations, migrations, development seeds | Empty SQL database migrates; 3+ artists, 8+ tracks, 3 DSPs; unique/FK constraints work; demo password hashed |
| 3 — Authentication | Login, refresh, logout, current-user endpoints; JWT validation; cookie handling; exact CORS | Real seeded user can log in; invalid/expired JWT rejected; refresh rotation, replay rejection, logout, cookie and CSRF behavior verified |
| 4 — Track API | Required artist/track endpoints, DTOs, services, repositories, validation and errors | Filters/details work; bad input, missing records, duplicate ISRC/distributions, and status rules checked; protected writes reject anonymous requests |
| 5 — React UI | Login screen plus two track views, API client and session flow | Real login works; status filter and details work; loading/empty/error/expired-session states; frontend validation matches API constraints |
| 6 — Hardening | Structured logging, correlation IDs, rate limits, final security pass | No sensitive logs; meaningful throttling responses; focused integration checks; full browser flow verified |
| 7 — Submission | README, DECISIONS, requirement checklist, submission-link instructions | Fresh setup/migration instructions verified; honest AI record; repo history and final links reviewed; user controls external submission |

## Confirmed backend structure
```text
Takwene.sln
backend/Takwene.Api/
  Controllers/
  DTOs/Auth/ Artists/ Tracks/
  Models/Enums/
  Data/AppDbContext.cs
  Data/Configurations/
  Data/Seed/
  Migrations/
  Repositories/Interfaces/
  Repositories/
  Services/Interfaces/
  Services/
  Validation/
  Middleware/
  Options/
  Program.cs
frontend/
Docs/
README.md
DECISIONS.md
```
No separate Domain/Application/Infrastructure projects. Test approach must respect the one-project solution rule; choose a suitable external integration harness or review any proposed test project with the user first.

## Proposed API contract
- POST /api/auth/login — email/password; returns access token and safe user info; sets refresh cookie.
- POST /api/auth/refresh — rotates refresh cookie and returns a new access token.
- POST /api/auth/logout — revokes refresh session and clears cookie; client clears access token.
- GET /api/auth/me — JWT protected.
- POST /api/artists — JWT protected; GET /api/artists — public.
- POST /api/tracks — JWT protected.
- GET /api/tracks?artistId=&genre=&status= — public; combine supplied filters.
- GET /api/tracks/{id} — public; includes artist and distribution/DSP details.
- POST /api/tracks/{id}/distribute — JWT protected; body contains dspIds.
- PATCH /api/tracks/{id}/status — JWT protected; body contains status.
- Confirmed: PATCH edits the track status independently of DSP distribution statuses; submitted -> distributed does not require a live DSP record. It does not update provider statuses or create submissions.

Use consistent ProblemDetails responses with field errors; 400 invalid input, 401 missing/invalid authentication, 404 missing route resource, 409 uniqueness or state conflict. Define referenced-ID validation consistently during API-contract review. Backend remains authoritative even when the frontend validates.

## Confirmed frontend authentication direction
- Store access JWTs in memory behind a small get/set/clear token-storage interface. Implement only the memory adapter initially; a future sessionStorage adapter can use the same interface.
- On application startup/reload, show an auth-initializing state and call refresh with credentials to restore the session. On refresh rejection, clear auth state and show login; distinguish network/server failures from an invalid session.
- API client reads access tokens through the interface and sends Authorization: Bearer. Serialize refresh calls within a tab, retry a failed protected request at most once, and prevent refresh retry loops.
- Clear token and user state on logout. Keep storage choice separate from refresh coordination; concurrent browser tabs share the refresh cookie, so review cross-tab rotation behavior during authentication implementation.
- SessionStorage is a future option, not a requirement for greater traffic volume. Switching requires revisiting XSS exposure, expiry cleanup, and startup behavior.

Step 3 implementation details, exact changed files and verification: STEP-3-REVIEW.md. Strict refresh replay revocation requires cross-tab refresh coordination in Step 5.
