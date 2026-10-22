# Refresh-token generator review

Updated: 2026-10-22 (Africa/Cairo).
Status: implemented and verified; awaiting user review before frontend.

## Delivered
New Services/Interfaces/IRefreshTokenGenerator.cs, Services/RefreshTokenGenerator.cs and Services/RefreshTokenResult.cs. AuthService delegates refresh-value generation and hashing. Program registers the service.

GenerateRefreshToken() returns an opaque Token and its TokenHash. The token uses 32 cryptographically random bytes encoded as uppercase hexadecimal; hashing uses SHA256 over the exact token value. HashToken(string) serves incoming refresh/logout database lookups. Algorithm and persisted format are unchanged.

AuthService retains initial seven-day expiry, created time, user/family metadata, preserving absolute expiry on rotation, replacement links, database transactions, replay revocation and logout. The generator performs no persistence and sets no cookies. Only hash goes into the model/database; the existing AuthSession carries the raw token to the HttpOnly cookie. Refresh tokens are not JWTs. No schema, package, endpoint, cookie policy or access-generator changes.

## Verification
- Build passes with zero warnings/errors.
- Expanded auth harness passes refresh-value format, computed SHA256-only persistence, initial seven-day lifetime, cookie/session expiry agreement, rotation without extending expiry, replacement links, replay family revocation, logout, expired refresh and concurrent rotation.
- Existing access-token claims/exact expiry, protected endpoints and full track regression pass.
- SQL constraints and original seed counts unchanged; EF reports no pending model changes.
- Test-created business fixtures cleaned up; revoked auth-test sessions remain. No secret/raw token values printed by tests.

## Remaining limits
No frontend/browser/certificate-trust verification yet. Local test clients skip certificate validation only. AutoMapper's documented development/testing license warning remains. Rate limiting/logging hardening remains Step 6. Strict refresh replay still requires cross-tab coordination in frontend.

References: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.getbytes and https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html.

Next: user review, then Step 5 frontend when authorized. Local milestone commit only; user handles push.
