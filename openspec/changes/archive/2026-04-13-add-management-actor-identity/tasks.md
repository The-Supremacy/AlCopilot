## 1. Backend: Actor Identity Capture

- [x] 1.1 Add a management actor identity representation at the Host boundary using authenticated claims, with stable user ID plus display value.
- [x] 1.2 Propagate management actor identity into management write paths without breaking module boundaries.

## 2. Backend: Persistence And Mapping

- [x] 2.1 Extend audit log persistence to store actor user ID separately from the existing display actor value.
- [x] 2.2 Extend import-owned workflow records that represent operator-triggered actions to store actor identity where appropriate.
- [x] 2.3 Add any required EF Core migrations and mapping updates.

## 3. Backend: Behavior Verification

- [x] 3.1 Add tests proving authenticated management mutations persist actor identity in audit logs.
- [x] 3.2 Add tests proving import start, review, apply, or decision-audit paths persist actor identity where required.
- [x] 3.3 Keep anonymous fallback behavior covered only for truly unauthenticated paths that remain supported.

## 4. Documentation

- [x] 4.1 Update relevant runtime or architecture docs if implementation decisions refine how the Host passes actor identity into modules.
- [x] 4.2 Keep this change aligned with management authentication guidance and follow up if additional modules need the same traceability pattern later.
