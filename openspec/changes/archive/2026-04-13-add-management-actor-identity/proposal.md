## Why

Management authentication now gives the backend a real authenticated operator identity, but the current management audit trail still records actions with a generic actor string such as `anonymous`.
That means important operator workflows like catalog mutations, import start, review, apply, and cancel do not preserve a stable user identifier that can be traced back to the authenticated manager or admin who performed them.

We need management audit and import metadata to record authenticated actor identity now so operational review, troubleshooting, and future accountability workflows have durable identity context instead of display-only text.

## What Changes

- Capture authenticated management actor identity at the Host boundary and make it available to management command handlers and audit writers.
- Extend management audit records to store a stable actor user ID alongside the current human-readable actor string.
- Record operator identity in import lifecycle metadata where user-triggered workflow actions are persisted, especially decision audit entries and import provenance or batch metadata.
- Keep anonymous fallback behavior only for flows that genuinely run without an authenticated management actor.

## Capabilities

### New Capabilities

- `management-actor-traceability`: persistent operator identity capture for management audit and import workflow records.

## Impact

- Affected code is expected in `server/src/AlCopilot.Host`, `server/src/Modules/AlCopilot.DrinkCatalog`, and backend tests.
- The change primarily affects backend persistence and traceability behavior rather than portal layout.
- A database migration is likely because audit and import-owned records need additional persisted identity fields.
- The change should preserve current human-readable audit presentation while adding stable actor identity for future reporting and filtering.
