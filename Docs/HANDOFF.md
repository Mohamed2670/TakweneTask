# Handoff

Updated: 2026-10-22 (Africa/Cairo).
Active workspace: E:\TakweneTask. Original E:\firstC#\TawkeneTask superseded. Read VISION, ROADMAP, SCHEMA, PROGRESS and STEP-4-REVIEW first.

## Current state
Step 4 artist/track API implemented and verified; awaiting review before Step 5 frontend. One .NET 10 project. React still foundation. SQL Express database TakweneTask has unchanged business seeds (3 artists, 9 tracks, 3 DSPs, 8 distributions, 1 user) plus revoked auth-test session records.

## Track API
Controllers -> services -> focused repositories. DTO normalization and model validation before saves. Central ApiExceptionHandler returns safe ProblemDetails. Artist/track creation and distribution/status writes require JWT. Reads public. Lists combine artistId/genre/status; blank optional text filters mean absent. Status enums persisted as lowercase strings and serialized as strings; no integer JSON enums.
Track mutations use a transaction plus sp_getapplock with track-specific resource to serialize across processes. Submission validates the full DSP set, adds pending distributions, advances draft->submitted atomically. Existing DSPs conflict whole request. PATCH follows draft->submitted->distributed, idempotent same status, no skip/backwards; independent from provider records. No migration change.

## Authentication retained
Access JWT 10 minutes, no SQL persistence. Opaque refresh hash in SQL; absolute seven-day family expiry; rotating cookie HttpOnly/Secure/SameSite=None/path /api/auth. Auth POSTs need HTTPS, exact Origin, X-CSRF:1. Local allowed origin http://localhost:5173. Strict replay revokes family, including a concurrent winning rotation. User secrets hold password/key; never print/commit. Frontend must coordinate refresh across tabs (Web Locks) in Step 5; keep access token in memory behind storage extension point.

## Verification / next
Track/auth scripts pass over local HTTPS; test clients skip certificate validation only. No browser/certificate-trust verification. Script verify-tracks clears WebSession headers to avoid accidental bearer reuse, creates isolated artist-owned fixtures and removes them. Persistence constraints and seed counts checked; EF snapshot consistent; audit checked.
User reviews Step 4, then frontend login/list/detail and refresh coordination. Final log/rate-limit hardening Step 6; lists currently unpaginated and lock timeout has generic server response. Do not expand scope without review.

Git on main; commit each completed step with Docs; never push unless asked. Verification server must be stopped before handoff. No automatic migration/seeding.
