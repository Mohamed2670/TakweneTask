# Step 3 review — enums and authentication

## Result
Added TrackStatus (Draft, Submitted, Distributed) and DistributionStatus (Pending, Live, Rejected). EF converts them to existing lowercase varchar values; no schema migration or seed rewrite needed. JSON uses lowercase enum names and rejects numbers.

Authentication now provides POST login, refresh, logout and JWT-protected GET me. Access tokens expire after 10 minutes. Random 256-bit refresh values are hashed in SQL, rotated under a database session-family lock, and tied to an absolute 7-day expiry. Replay revokes the family. Replacement links are recorded. Normal login rehashes outdated password hashes when needed.

Cookies: __Secure-Takwene.Refresh; HttpOnly; Secure; SameSite=None; path=/api/auth. Auth POSTs require HTTPS, an exact allowed Origin and X-CSRF: 1. CORS permits only configured origins with explicit methods/headers. Responses disable caching. No registration/roles introduced.

## Added files
- Models/Enums/TrackStatus.cs
- Models/Enums/DistributionStatus.cs
- DTOs/Auth/LoginRequest.cs
- DTOs/Auth/AuthResponse.cs
- Options/JwtOptions.cs
- Repositories/Interfaces/IAuthRepository.cs
- Repositories/AuthRepository.cs
- Services/Interfaces/IAuthService.cs
- Services/AuthService.cs
- Validation/AuthRequestFilter.cs
- Controllers/AuthController.cs
- scripts/verify-auth.ps1

Backend paths above are relative to backend/Takwene.Api. Existing models, EF mappings, seed data, Program.cs, development settings, project dependencies, README, DECISIONS and Docs were updated. DevelopmentUserSeeder now caps password length at 256 to match login validation. Signing key was generated into local user secrets, outside source.

## Evidence
- Build: zero warnings/errors.
- EF model unchanged at provider level; no pending model changes.
- Integration checks: valid/invalid login; trimmed email; input validation; authenticated/anonymous requests; malformed, expired, wrong-issuer and wrong-audience JWTs; allowed/disallowed CORS; CSRF header/origin rejection; cookie attributes; refresh rotation; replay family revocation; logout; expired refresh; simultaneous refresh serialization and strict reuse revocation.
- Persistence constraints and seed counts rerun; transitive package audit rerun.

## Limits and next step
No browser cookie flow verified. Test HTTP clients skip local certificate validation; trust remains unverified. SameSite=None accommodates HTTP frontend with HTTPS API; final deployment should review same-site hosting/cookie policy. React refresh coordination across tabs is required in Step 5. Logging hardening/rate limits remain Step 6. Access JWTs remain valid until expiry after logout. Normal endpoints do not immediately revoke JWTs for disabled users; me checks active users, and disabled users cannot renew.

Await user review before Step 4 artist/track API implementation.
