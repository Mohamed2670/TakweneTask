# DTO mapping refactor plan

Updated: 2026-10-22 (Africa/Cairo).
Status: implemented with user-selected AutoMapper 16.2.0; verification passed; implementation review pending.

## Structure
Add backend/Takwene.Api/Profiles with ArtistProfile.cs, TrackProfile.cs and AuthProfile.cs. Register profiles once through dependency injection. Services use IMapper for structural conversions, while retaining validation and business rules.

| Profile | Source | Destination | Rules |
| --- | --- | --- | --- |
| ArtistProfile | CreateArtistRequest | Artist | Map name/email/country; ignore generated Id and Tracks navigation |
| ArtistProfile | Artist | ArtistResponse | Public artist fields only |
| TrackProfile | CreateTrackRequest | Track | Map validated input; ignore Id, Status, Artist and Distributions; service sets Draft explicitly |
| TrackProfile | Track | TrackListResponse | Flatten Artist.Name into ArtistName |
| TrackProfile | Track | TrackDetailResponse | Flatten artist name, map nested distributions ordered by DspId |
| TrackProfile | TrackDistribution | DistributionResponse | Flatten Dsp.Name; preserve SubmittedAt as UTC |
| AuthProfile | User | UserResponse | Only Id/Name/Email; no password hash, flags, sessions or token data |

## Boundaries
Mappings are one-way, with no ReverseMap. Request normalization/validation remains outside profiles and runs before mapping. Services retain status-transition checks, explicit default status, creation timestamps, FK/ISRC checks and transactions. LoginRequest is a credential input, not a User creation model. DistributeTrackRequest fans out through service logic, not an entity map. PATCH status remains an explicit assignment after validation; no map onto an existing tracked entity that could bypass business rules. AuthResponse and refresh records combine generated security data and remain constructed by auth/token code; only their nested UserResponse uses mapping.

Initially use in-memory IMapper.Map over the existing eager-loaded repository results. Do not introduce ProjectTo or repository/SQL projection changes in the same refactor. Response contracts and migrations remain unchanged.

## Verification
Validate mapping configuration, including record constructor bindings and ignored destination members. Run build plus existing track/auth integration harnesses and EF consistency check. Add focused checks for flattened names, sorted nested collections, UTC handling and absence of sensitive User fields where needed. Commit the refactor and updated Docs locally after verification, then pause for review.

## Library decision
User explicitly selected AutoMapper 16.2.0. AutoMapper is the only mapping library installed; temporary uncommitted Mapster references were removed. AutoMapper 16.2.0 emits a missing-license warning permitting development/testing; no license was supplied, no warning is suppressed, and production licensing remains to be configured. Optional key comes from AutoMapper:LicenseKey configuration or the library's documented environment lookup, never source code.
Sources: https://docs.automapper.io/en/latest/License-configuration.html and https://docs.automapper.io/en/stable/Dependency-injection.html.

This refactor is an extra review step before Step 5 frontend. See MAPPING-REVIEW.md for changed files and verification.
