# SQL Server schema

Status: persistence reviewed; Step 3 adds C# status enums without changing SQL schema. IDs are int identity unless noted. Required fields are NOT NULL unless marked nullable. Store timestamps as UTC datetime2; ReleaseDate is SQL date. Configure maximum lengths, foreign keys, indexes, check constraints, and delete behavior explicitly through EF Core configurations.

| Table | Columns |
| --- | --- |
| Users | Id; Name nvarchar(100); Email nvarchar(254); NormalizedEmail nvarchar(254) unique; PasswordHash nvarchar(512); IsActive bit; CreatedAt datetime2 |
| RefreshTokens | Id; UserId FK; TokenHash char(64) unique; FamilyId uniqueidentifier; CreatedAt datetime2; ExpiresAt datetime2; RevokedAt datetime2 nullable; ReplacedByTokenId int nullable self FK; RowVersion rowversion |
| Artists | Id; Name nvarchar(150); Email nvarchar(254); Country nvarchar(100) |
| Tracks | Id; Title nvarchar(200); ArtistId FK; Isrc varchar(12) unique; ReleaseDate date; Genre nvarchar(100); Status varchar(20) |
| DSPs | Id; Name nvarchar(100) unique |
| TrackDistributions | Id; TrackId FK; DspId FK; SubmittedAt datetime2; Status varchar(20); unique(TrackId, DspId) |

## Relationships and constraints
- Artist 1:N Tracks; Track 1:N TrackDistributions; DSP 1:N TrackDistributions.
- User 1:N RefreshTokens; tokens have no relationship to artists in this demo. Users are staff operators, not artist accounts.
- Restrict deletes for business relationships and refresh-token replacement references. No delete endpoints are required.
- Track status check: draft, submitted, distributed. Distribution status check: pending, live, rejected.
- Index foreign keys, NormalizedEmail, TokenHash, and FamilyId. Revisit filter indexes after inspecting actual query needs.
- No password plaintext or raw refresh token is persisted. Use a framework password hasher, not a custom hash.
- Access JWTs have a confirmed 10-minute lifetime and are not persisted. No access-token table is planned. Future session/audit analytics can be modeled independently without storing raw JWTs.

## Implemented input normalization and validation
- Use request DTO rules plus EF constraints and service business checks. Validate frontend inputs for immediate feedback.
- Trim names, emails, titles, country, genre, and ISRC. Never trim passwords or change password characters.
- Reject whitespace-only required values and values above configured lengths. Validate email shape without claiming deliverability.
- Canonicalize user email for lookup; preserve a display email. Artist email uniqueness is not required by the assignment and is not imposed here.
- Normalize ISRC to uppercase and remove accepted display hyphens before validating its canonical 12-character format: two letters, three alphanumeric characters, seven digits. Enforce database uniqueness and translate races to a conflict response.
- Validate real calendar dates; future release dates are allowed. Validate positive IDs, supported status values, and existing foreign keys.
- Distribution requests require at least one distinct valid DSP ID; validate the complete request before writing. Use a transaction for submissions and track status changes.

## Implemented business behavior — Step 4 review pending
- New tracks start as draft; creation DTO does not accept arbitrary status.
- Distribution may submit a draft or submitted track to DSPs it does not already target; new rows are pending. A draft moves to submitted in the same transaction.
- Duplicate DSP IDs within a request are invalid. A previously targeted DSP produces 409 for the whole request, with no partial writes. Retry/rejection resubmission is outside initial scope.
- Confirmed adjustment: PATCH /api/tracks/{id}/status can manually move submitted -> distributed without requiring a live DSP distribution. It updates only Tracks.Status, never TrackDistributions.Status, and does not create distribution records.
- Implemented transition policy: draft -> submitted -> distributed; no backward transitions or direct draft -> distributed initially. Same-status requests succeed without mutation. Validate status values and return 409 for disallowed transitions.
- Distribution submission never pretends content is live. No real DSP callback or distribution-status update endpoint is required. Seed data supplies live/rejected examples; review whether a demo simulation endpoint is desired before expanding scope.
- Distributed tracks cannot be resubmitted under the initial rules. These rules are assignment implementation decisions; Step 4 report is ready for user review.

## Development seed proposal
- Three or more fictional MENA artists with reserved example.com email addresses.
- Eight or more tracks spanning genres and all three track statuses; unique canonical ISRCs.
- Spotify, Apple Music, YouTube DSPs; pending/live/rejected distribution examples consistent with track statuses.
- One active demo staff user. Supply its password through local development configuration; hash it using the framework hasher. Document how to set it in README.
- Keep model configuration and seeds under Data. Choose migration-safe deterministic business seeds and a separate idempotent development-user initializer where password hashing/configuration requires runtime execution. No production demo credentials or automatic production migrations.


Status enums: Models/Enums/TrackStatus and DistributionStatus. EF persists lowercase strings and retains SQL check constraints. Auth uses refresh-token hashes with replacement links and strict family replay revocation. Session lifetime is absolute seven days; auth integration tests may leave revoked rows in the local development database.
