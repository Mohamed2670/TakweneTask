# Decisions and AI contribution record

This is a living record, not the final submission.

## AI and human contributions
The user selected SQL Server, .NET, React, the single-project repository/service architecture, tracking rules, and per-step reviews. The user approved in-memory access tokens with refresh on reload and manual track-status updates.
AI drafted planning documents and generated the initial backend/frontend foundation. No claim is made that the user authored or modified application code.

## Security review so far
No authentication is implemented yet. Planned controls include password hashing, hashed/rotating refresh tokens, HttpOnly cookies, CSRF protection, and explicit CORS origins. Access JWTs will not be stored in SQL Server. No credentials were added.

## Actual AI corrections
The initial scaffolding invocation produced a plain TypeScript app instead of React. Inspection of package.json exposed missing React dependencies and entry point; AI corrected the app to React and added TypeScript React support.
An attempted Vite configuration-loader workaround failed because the workspace path contains #. A native JavaScript config permits builds, but development source loading remains blocked. This limitation is recorded rather than claimed resolved.

## Architecture
One backend project with controllers, DTOs, models, services, repositories, and EF data configuration follows the user's confirmed choice. Folder boundaries are not enforced with separate assemblies.

## Workspace relocation follow-up
Active workspace: E:\TakweneTask. User approved relocation. Windows locked the original directory, so sources were copied without generated dependencies/build output. Original remains intact and is superseded.
Verified at new path: npm ci; frontend production build; dotnet build with zero warnings/errors; Vite startup; HTTP 200 for HTML, transformed React entry, React and React DOM dependencies. The # path blocker is resolved. Visual browser verification not performed. Verification server stopped.
Await user review before Step 2 persistence. Earlier blocker/environment notes describe the original path and are superseded by this entry.

## Step 2 implementation record
AI generated six EF models/configurations, migration, deterministic seeds, development user seeding and SQL verification. It added RowVersion to refresh tokens for future concurrency control; SCHEMA.md updated accordingly.
Dependency audit exposed vulnerable System.Security.Cryptography.Pkcs transitive versions from the SQL client dependency graph. Updating SQL client alone did not resolve it; an explicit Pkcs 10.0.13 reference did. Build and transitive audit subsequently passed. No sensitive-data logging is enabled; password is hashed and initial password is local user-secret configuration.
No request DTO validation or auth endpoints exist yet; model annotations alone do not validate EF saves. These controls remain for subsequent milestones.

## Step 3 AI contributions and corrections
AI added status enums and mappings, auth DTOs/options/controllers/repository/services, origin/custom-header validation and integration tests. User requested enums and approved proceeding from persistence.
Security choices: strict JWT claims/signature/expiry validation, random refresh values with SHA256 hashes, secure HttpOnly cookies, origin plus custom-header CSRF checks, exact credentialed CORS, noncached responses, generic login errors, and dummy password verification for unknown users. Guidance consulted: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html and https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0.
Review caught an initially unpopulated ReplacedByTokenId link in rotation; AI added its update within the session transaction and verified links in SQL. A PowerShell test expression needed parentheses for timestamp arithmetic before expiry tests could run. Tests now pass.
Known tradeoffs: strict replay detection logs out a session if tabs refresh concurrently; frontend must coordinate. SameSite=None supports mixed-scheme local frontend/API topology but browser behavior still needs verification. Local test clients skip certificate validation; application does not. Rate limiting remains planned for Step 6.

## Git history provenance
Git was initialized after Steps 1–3. AI reconstructed planning/foundation/persistence commits from the retained original foundation and current files, then committed current enums/auth. Commits use actual creation dates, not backdated dates. See Docs/GIT-HISTORY.md. The user authorized local milestone commits and will push to origin after completion.
