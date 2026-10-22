# Mapping refactor review

Updated: 2026-10-22 (Africa/Cairo).
Status: implemented and verified; awaiting user review before Step 5 frontend.

## Delivered
AutoMapper 16.2.0, explicitly selected by user. Mapster packages removed. Added backend/Takwene.Api/Profiles/ArtistProfile.cs, TrackProfile.cs, AuthProfile.cs and MappingRegistration.cs. Program registers profiles and validates mapping configuration at startup. ArtistService, TrackService and AuthService inject IMapper for structural conversions.

| Source | Destination | Notes |
| --- | --- | --- |
| CreateArtistRequest | Artist | Ignore Id and Tracks |
| Artist | ArtistResponse | Public fields |
| CreateTrackRequest | Track | Ignore Id, Status, Artist, Distributions; service explicitly sets Draft |
| Track | TrackListResponse | Artist name flattened |
| Track | TrackDetailResponse | Artist name flattened; distributions ordered by DspId |
| TrackDistribution | DistributionResponse | DSP name flattened; SubmittedAt UTC |
| User | UserResponse | Id, Name, Email only |

Validation/normalization remains before mapping. Status transitions, timestamps, distribution fan-out, passwords/tokens, FK checks and transactions remain service responsibilities. No ReverseMap or automatic mapping onto tracked update entities. AuthResponse combines generated token data explicitly and maps only its nested UserResponse. Repositories, request/response contracts, schema and frontend unchanged.

## Verification
- Build: zero warnings/errors. Startup AssertConfigurationIsValid passes.
- Existing track/auth integration suites pass; added assertions verify list/detail artist names, nested order, DSP name, UTC output and user field allowlist.
- SQL persistence constraints pass; original counts remain 3 artists, 9 tracks, 3 DSPs, 8 distributions, 1 user.
- EF reports no pending model changes. NuGet transitive vulnerability audit reports none.
- Business fixtures cleaned up; revoked auth-test session records remain as documented.

## Changed files
Added four Profiles files. Updated Program.cs, project package references, ArtistService.cs, TrackService.cs, AuthService.cs, scripts/verify-auth.ps1 and scripts/verify-tracks.ps1, README, DECISIONS and tracking Docs.

## Licensing and limits
AutoMapper's missing-license warning is not suppressed. Its runtime message permits development/testing; production requires an applicable license. Optional AutoMapper:LicenseKey is read from server configuration, never hardcoded. No key was supplied. Official licensing documentation: https://docs.automapper.io/en/latest/License-configuration.html.
No browser/cookie/certificate-trust verification in this refactor. Local HTTP test clients skip certificate validation; application code does not. Next milestone remains gated on review.
