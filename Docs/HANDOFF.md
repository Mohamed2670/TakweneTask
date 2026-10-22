# Handoff

Updated: 2026-10-22 (Africa/Cairo).
Active workspace: E:\TakweneTask. Original E:\firstC#\TawkeneTask is superseded. Read VISION, ROADMAP, SCHEMA, PROGRESS and STEP-3-REVIEW first.

## State / next action
Step 3 enums/auth complete; awaiting user review before Step 4 artist/track endpoints. Exactly one .NET 10 project. Database TakweneTask on SQLEXPRESS contains initial seeds and test refresh sessions. React remains foundation only.

## Auth implementation
Access JWT: 10 minutes, no SQL persistence; issuer/audience/signature/lifetime/HS256 validation, zero clock skew. Refresh: 256-bit opaque value, SHA256 hash only, secure HttpOnly SameSite=None cookie scoped /api/auth. Exact origins from config and X-CSRF:1 required for auth POSTs over HTTPS. Local origin http://localhost:5173. 7-day absolute family expiry. SQL sp_getapplock serializes rotations/logout across instances; replay revokes family including a concurrent winning rotation. ReplacedByTokenId populated; RowVersion present. User secrets contain demo password and JWT key; never print/commit. Cookie lifetime and DB expiry agree. No registration/roles.

## Enum implementation
Models/Enums defines TrackStatus and DistributionStatus. EF maps lowercase varchar strings, JSON maps camelCase and rejects numbers. Existing initial migration unchanged; model check passes.

## Verification / limitations
Auth integration harness passes; requires API HTTPS running and reads local secrets silently. Harness skips certificate validation only for test clients and changes one test-created refresh row for expiry; not browser verification. Actual cookie sending with HTTP frontend/HTTPS API not verified. Review same-site topology before deployment. Frontend must serialize refresh across tabs (Web Locks) to avoid strict replay-triggered logout. Browser storage adapter remains for Step 5. Logging/rate limits Step 6. See STEP-3-REVIEW for file list and full checks.

Build and migration checks pass. SQL service running. Git is initialized on main with focused local milestone commits. Earlier history is reconstructed; see GIT-HISTORY.md. User handles pushing. Verification server should be stopped at end of turn. No automatic migrations/seeding. DataAnnotations alone do not validate EF saves; API validation/normalization still needed for track endpoints.

Git rule confirmed 2026-10-22: commit each completed milestone locally, including Docs updates; leave pushing to the user. See GIT-HISTORY.md. Committing does not imply user approval of Step 3.
