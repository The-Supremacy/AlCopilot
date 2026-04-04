## Why

The `/opsx:verify` report identified a divergence between the drink-browsing spec and the implementation. The spec states tag filtering uses AND logic (drinks must have ALL specified tags), but OR logic (drinks matching ANY specified tag) is more intuitive for discovery-oriented browsing. Users searching with multiple tags typically want to broaden results, not narrow them. The current implementation already uses OR — the spec needs to align, and a multi-tag integration test is needed to verify the behavior.

## What Changes

- Update the drink-browsing spec to document OR logic for tag filtering instead of AND
- Add a multi-tag integration test that exercises the OR behavior (current single-tag test cannot distinguish AND from OR)

## Capabilities

### New Capabilities

_(none)_

### Modified Capabilities

- `drink-browsing`: Tag filtering requirement changes from AND logic to OR logic

## Impact

- **Spec**: `openspec/specs/drink-browsing/spec.md` — one line change (AND → OR)
- **Tests**: `server/tests/AlCopilot.DrinkCatalog.Tests/` — new multi-tag integration test
- **Code**: No code changes needed — `DrinkRepository.GetPagedAsync` already implements OR logic
