## 1. OpenSpec And Requirements Alignment

- [x] 1.1 Create the dedicated frontend-testing follow-up change artifacts for the management portal.
- [x] 1.2 Remove stale ingredient-category requirements from the active management-portal change and keep archived OpenSpec history untouched.

## 2. Frontend: Import Workspace Alignment

- [x] 2.1 Update the Imports workspace state so apply is enabled only when validation has no errors and every conflict has an explicit stored decision from review.
- [x] 2.2 Keep review decisions cleared after successful apply or cancel flows.

## 3. Frontend: Management Portal Tests

- [x] 3.1 Add page tests for ingredient create and edit workflows, including parsed brand lists, navigation, error, delete, and not-found states.
- [x] 3.2 Add page tests for drink create and edit workflows, including normalized payloads, filtered recipe entries, loading, delete, and not-found states.
- [x] 3.3 Add page tests for import review behavior covering stale review refresh, decision editing, and completed/cancelled batch behavior.
- [x] 3.4 Add page-hook or page tests for imports workspace apply/cancel behavior using stored decisions and blocked-state hints.

## 4. Verification

- [x] 4.1 Search non-archived OpenSpec artifacts to confirm ingredient-category requirements are removed from live specs.
- [x] 4.2 Run the management-portal frontend test suite after the new coverage is added.
