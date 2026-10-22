# Git history and commit rules

Updated: 2026-10-22 (Africa/Cairo).

The user authorized Git initialization and a local commit for each completed step. The user will push to origin after finishing; do not push automatically.

## History provenance
Git was initialized after Steps 1–3 had been implemented. Initial planning/foundation/persistence commits are reconstructed from the retained original foundation and the current implementation. They are not contemporaneous development snapshots and are not backdated. Authentication/status enums follow as the final reconstruction commit. Intermediate documentation is reconstructed to describe each milestone, rather than claiming an original saved version. Current final files remain authoritative.

## Ongoing rule
Keep each completed milestone in a focused local commit. Include its tracking/documentation updates and meaningful verification. Record review state separately; creating a commit does not mean the user approved its implementation. Never include secrets or generated build/dependency output. Leave pushing and remote configuration to the user unless explicitly asked.
Verification: foundation, persistence, and current authentication commits each build from clean temporary exports. A reconstruction-only project-file encoding error was caught and corrected before finalizing the local history. Local secret values were checked against source candidates without printing them; no ignored build/dependency files are tracked. Working tree was checked clean.
