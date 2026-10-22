# Handoff

Updated: 2026-10-22 (Africa/Cairo).
Active workspace: E:\TakweneTask. Original E:\firstC#\TawkeneTask superseded.
Read VISION, ROADMAP, SCHEMA, PROGRESS and STEP-5-REVIEW first.

## Current state / review gate
Steps 1–4c complete; user authorized Step 5. React login/library/detail implemented and verified, pending user review. Do not begin Step 6 until approved. Exactly one .NET project; no DDD project split. SQL Express database TakweneTask retains business seeds: 3 artists, 9 tracks, 3 DSPs, 8 distributions, 1 user. Test-created refresh rows remain revoked.

## Run locally
From the root: dotnet run --project backend/Takwene.Api --launch-profile https.
From frontend: npm ci, then npm run dev. Ports: API https://localhost:7270, frontend http://localhost:5173. Default VITE_API_URL=https://localhost:7270/api; .env.example documents override. Local HTTPS certificate is trusted and real browser cookie restoration passed. Servers stopped before handoff.

## Frontend structure
src/api contains typed HTTP/error handling, authenticated request retry and track queries. src/auth contains the memory token adapter, external session store and React hook. Components provide layout/status/feedback/artwork; pages provide login/list/detail; useResource handles cancellation/retry. App defines protected routes and error boundaries; style.css handles desktop/mobile layouts. Tests live outside src in frontend/tests.

Login trims email but never passwords. URL status filtering requests server data; search operates locally over that small result. Detail links preserve the status-filter back path. No catalog mutation forms added. API reads remain public even though the UI requires a staff session.

## Authentication
Access JWT: 10 minutes, memory only, no SQL persistence. The get/set/clear adapter supports a future storage change; sessionStorage is not implemented. Refresh cookie: HttpOnly/Secure/SameSite=None, path /api/auth; hash only in SQL, rotation with absolute seven-day lifetime, strict family revocation on reuse. HTTPS auth POSTs require exact Origin plus X-CSRF:1 and credentialed fetch.

A shared promise coalesces refreshes per tab; Web Locks serialize login/refresh/logout across same-origin tabs. BroadcastChannel sends only sign-in/sign-out signals, never tokens. Browser must support Web Locks in a secure context; no unsafe fallback. Startup distinguishes invalid sessions from service/network failure and offers retry. API client refreshes expired memory tokens and retries a 401 once; 403 does not refresh. Successful logout clears memory and other tabs; failed logout does not claim success.

User secrets hold Jwt:SigningKey and Seed:DemoPassword. User selected a local demo password; never print or commit it. Development seed accepts 6–256 characters. Normal startup does not seed/reset/migrate. To change an existing demo password, stop the API, update the secret, run --reset-demo-password with the Development http launch profile, then restart. This explicit command hashes the password and revokes active refresh sessions; old access JWTs still expire normally. --seed-demo-user remains idempotent for an existing user. No public registration policy introduced.

## Backend retained
Controllers -> services -> focused repositories; AutoMapper 16.2.0 profiles handle seven conversions. Validation/business rules/transactions stay explicit in services. ITokenGenerator uses SecurityTokenDescriptor, HS256, sub/email/jti and exact 600-second expiry. IRefreshTokenGenerator creates cryptographic opaque values plus SHA256 hashes; AuthService owns session policy. No new migration/schema changes.

Track writes use SQL transactions plus track-specific application locks. Distribution is atomic, creates pending records and advances draft to submitted. PATCH status follows draft -> submitted -> distributed, same status idempotent, independent of DSP status. Read filters combine artist/genre/status. Safe ProblemDetails errors; reads public, writes JWT protected. Earlier milestone reports retain exact API checks.

## Verification / limitations
npm test: 22 tests; npm run build: passes. Backend build and both scripts/verify-auth.ps1 and verify-tracks.ps1 pass. Browser checks cover real login, reload, simultaneous tab reloads, cross-tab sign-in/out, logout persistence, status/search/detail, mobile overflow and API outage/retry. No full compatibility or accessibility audit claimed. Browser storage contents were not inspected; memory-only behavior is supported by source and adapter tests.

React Router's use-client bundling warning is harmless for this client-only SPA and remains visible. AutoMapper emits its missing-license development warning; production licensing is unresolved, do not suppress it. Configure a valid AutoMapper:LicenseKey if applicable. No pagination or real DSP integration. Deployment requires SPA route fallback and reviewing exact CORS/cookie topology.

Next: user reviews Step 5, then logging/correlation/rate limits in Step 6; submission finalization in Step 7. Git main; commit every completed step with Docs, never push automatically.
