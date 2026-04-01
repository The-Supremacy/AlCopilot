## 1. Documentation Updates

- [x] 1.1 Update `openspec/specs/drink-browsing/spec.md` — change "ALL specified tags (AND logic)" to "ANY of the specified tags (OR logic)" in the tag filter scenario
- [x] 1.2 Add "Browse drinks filtered by multiple tags returns union" scenario to the main spec

## 2. Integration Test

- [x] 2.1 Add multi-tag OR filter integration test to `DrinkIntegrationTests` — seed two drinks with different tags, filter by both tag IDs, assert both drinks are returned
