# Token generator refactor plan

Updated: 2026-10-22 (Africa/Cairo).
Status: proposal awaiting user review; no application code changed.

## Proposed structure
- Services/Interfaces/ITokenGenerator.cs: GenerateAccessToken(User user).
- Services/TokenGenerator.cs: JWT generation through JwtSecurityTokenHandler and SecurityTokenDescriptor.
- Services/AccessTokenResult.cs: internal result containing Token and ExpiresAt, not an API response DTO.
- AuthService injects ITokenGenerator, composes AuthResponse with the result and AutoMapper UserResponse, and retains credential verification/refresh-session business logic.
- Program registers the generator through dependency injection.

## Descriptor fields
Issuer/Audience/SigningCredentials from existing validated JwtOptions. HS256 signing. Capture one UTC time for IssuedAt, NotBefore and Expires; align the returned ExpiresAt with the JWT expiry's whole-second precision. Lifetime remains the approved 10 minutes, not the example's 60-minute default.

Claims: explicit standard sub (user ID), email (existing user email), jti (unique token ID). Use sub explicitly because JWT validation has MapInboundClaims=false and /api/auth/me currently reads sub. User has no Role or BranchId, so do not add those claims or model fields as part of this refactor. No passwords, hashes or refresh tokens in the JWT.

## Lifetime boundaries
Recommended access-specific method rather than expiresInMinutes/expiresInDays arguments. Keep the duration centrally controlled by JwtOptions.AccessTokenMinutes. Optional duration overrides and day-based access JWTs are not needed in the assignment and would change the agreed policy.
Refresh tokens remain opaque cryptographically random values, hashed in SQL, rotated under a session-family transaction, with a seven-day absolute lifetime. Do not convert refresh tokens to JWTs through the access-token generator. No changes to cookies, refresh storage, roles, branches or database schema.

## Verification and delivery
Build, existing auth/track integration suites, JWT claims/lifetime/response-expiry checks, EF model consistency. Update Docs, README and DECISIONS, commit the completed refactor locally, pause for review before frontend.

User requested planning, not implementation. Resolve the proposed method shape and claims before changing code.
