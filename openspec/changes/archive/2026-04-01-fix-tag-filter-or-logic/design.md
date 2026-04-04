## Context

The DrinkCatalog module's `DrinkRepository.GetPagedAsync` already implements OR logic for tag filtering (`Tags.Any(t => tagIds.Contains(t.Id))`). The drink-browsing spec incorrectly documents AND logic. The existing integration test uses a single tag ID, which cannot distinguish between AND and OR behavior.

## Goals / Non-Goals

**Goals:**

- Align the drink-browsing spec with the intended OR behavior
- Add a multi-tag integration test that proves OR logic works correctly

**Non-Goals:**

- Changing the repository implementation (it's already correct)
- Adding a configurable AND/OR parameter (not needed now)
- Modifying any domain model, handler, or endpoint

## Decisions

### 1. Keep OR logic as-is

OR logic is the better default for a discovery-oriented browsing experience. Selecting multiple tags broadens the result set, helping users explore drinks. AND logic would narrow results and risks returning empty sets when tags are orthogonal.

**Alternative considered:** Configurable AND/OR via query parameter — deferred as over-engineering for current needs.

### 2. Multi-tag test strategy

Create one integration test that:

- Seeds two drinks with different tags (Drink A = Tag1, Drink B = Tag2)
- Filters by both Tag1 and Tag2
- Asserts both drinks are returned (proving OR, since AND would return neither)

This is the minimal test that definitively distinguishes OR from AND.

## Risks / Trade-offs

- [Future AND requirement] → Can add a `tagFilterMode` query parameter later without breaking the OR default
- [Spec drift] → The synced main spec at `openspec/specs/drink-browsing/spec.md` also needs updating (will be handled via delta spec sync)
