# Vision and working agreement

Planning version: 0.6 — 2026-10-22 (Africa/Cairo).
Status: Step 4 API complete; requested mapping refactor implemented and awaiting review before frontend.

## Goal
Deliver the Takwene Track Management API and React UI within the assignment scope. SQL Server stores artists, tracks, DSPs, distributions, and authentication data. The UI provides login, track list with status filtering, and track detail with DSP statuses. DSP integration is simulated through database records.

## Confirmed user choices
- SQL Server, .NET controller-based Web API, React frontend.
- Exactly one backend project in the solution. Repository/service pattern; controllers provide the MVC controller role without views. No DDD project structure.
- User explicitly reconfirmed the folder structure in ROADMAP.md. Architecture choice is settled.
- Access JWTs expire after 10 minutes and are not stored in SQL Server. Only refresh-token hashes are persisted.
- EF Core migrations, explicit relationship configurations, realistic seed data.
- AutoMapper 16.2.0, explicitly selected by user; one-way mappings centralized in Profiles. Keep validation and business/security operations in services.
- Backend validation of persistence models and request DTOs; frontend input validation; trim applicable text inputs.
- Access token in React memory, with refresh on page reload; refresh token in an HttpOnly cookie. Keep a small frontend token-storage interface to permit a future switch to sessionStorage without rewriting auth consumers.
- Explicit frontend CORS allowlist, including exact localhost development origins.
- Logging and rate limiting in the final hardening milestone.
- Each completed milestone requires user review before starting the next milestone.

## Main tracking rule
Docs is the source of truth for where we are, what is complete, what is next, and how another session continues. Read VISION.md, ROADMAP.md, PROGRESS.md, and HANDOFF.md at session start. Update PROGRESS.md and HANDOFF.md after meaningful work and before ending a session. Update schema and plan when an approved decision changes them. Mark proposals, implemented work, tested work, and user approval separately. Never claim approval or verification without evidence.

## Workflow rules
1. Present milestone scope, acceptance checks, and any additional rules before implementing it.
2. Complete only the current approved milestone, verify it, and report changed files, results, limitations, and decisions for user review.
3. Pause at the review gate. User review is required by the user's workflow instruction.
4. Keep controllers thin; services own business rules; repositories own database queries and persistence. Use DTOs rather than binding EF entities directly.
5. Use focused repositories; avoid generic frameworks and unnecessary abstractions. Agree on transaction ownership before implementation.
6. Keep secrets out of tracked files and logs. Never log passwords, tokens, or cookie values.
7. Track actual AI contributions, security findings, and corrections as they occur for DECISIONS.md. Do not invent user contributions or mistakes.
8. User authorized local commits for every completed milestone. Include tracking updates in each commit. The user will push to origin after completion; do not push automatically.

## Assignment alignment — decision recorded
The assignment explicitly requires Clean Architecture. The user reconfirmed one project with the documented layered folder structure. Proceed with that choice; do not reopen the project-count decision. Describe the architecture honestly in README and DECISIONS, including that folder boundaries are not enforced through separate assemblies.

## Security clarification
User approved in-memory access-token storage with refresh on reload. Memory avoids browser-storage persistence but does not prevent XSS. A future sessionStorage adapter is an extension point, not an implemented feature or an automatic scaling requirement. Do not duplicate the access token into a cookie without an explicit decision.

Implemented authentication defaults: 10-minute JWT access token; 7-day cryptographically random opaque refresh token; hash refresh tokens in SQL Server; rotate them on refresh; revoke their token family on reuse. Access JWTs travel in Authorization: Bearer. Refresh cookies use HttpOnly, Secure, explicit SameSite, and a scoped path. Protect auth POSTs through an exact allowed Origin and X-CSRF: 1 custom header over HTTPS. The cookie uses SameSite=None for current development topology. Use local HTTPS to exercise secure cookies.

Access-token database persistence is explicitly excluded. Validate JWT signature, issuer, audience, and expiration without a per-request token lookup. Browser storage is confirmed as memory; database persistence and browser storage are distinct decisions. Under this design, revoking refresh tokens prevents renewal but does not immediately invalidate an already-issued access token.

Future scaling: consider separate session/audit or request analytics records for user/session IDs, timestamps, request targets, and appropriate IP metadata. Traffic tracking does not require storing raw access JWTs; IP addresses alone do not establish a user's physical location. Analytics and immediate JWT revocation are outside the current agreed scope.

Sources:
- https://cheatsheetseries.owasp.org/cheatsheets/HTML5_Security_Cheat_Sheet.html
- https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0

## Open decisions
- Step 4 implements the documented schema/status defaults; review report records behavior and limitations.
- Seeded staff login implemented; no public registration or roles in scope.
- Task receipt/deadline remains unknown. Runtime/SQL/ports are recorded in handoff; local HTTPS certificate exists, browser trust is unverified.


Git rule confirmed 2026-10-22: commit each completed milestone locally, including Docs updates; leave pushing to the user. See GIT-HISTORY.md. Committing does not imply user approval of the current milestone.
