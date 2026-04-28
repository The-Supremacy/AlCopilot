## Frontend

- [x] Pre-apply gate: confirm `web/apps/web-portal/DESIGN.md` has the progressive-disclosure recommendation invariant before implementation.
- [x] Update `web/apps/web-portal/src/features/recommendations/RecommendationTurnList.tsx` so structured recommendation results render under assistant narration as collapsed progressive disclosure.
- [x] Render restock-oriented recommendation groups with the customer-facing label `Buy next`.
- [x] Render `Available now` and `Buy next` groups only when their DTO item collections are non-empty.
- [x] Add or update component tests for default collapsed groups, group expansion, per-drink expansion, conditional group rendering, and hidden scores.
- [x] Verify the implementation uses Tailwind CSS and existing shadcn/ui-aligned primitives, or document any approved exception.

## Validation

- [x] Run the focused web portal recommendation tests.
- [x] Run the web portal typecheck/build command if feasible.
- [x] Run OpenSpec validation for `improve-recommendation-result-disclosure`.
