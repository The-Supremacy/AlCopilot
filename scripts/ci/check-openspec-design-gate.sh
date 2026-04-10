#!/usr/bin/env bash
set -euo pipefail

changes_root="openspec/changes"

if [[ ! -d "$changes_root" ]]; then
  echo "No openspec changes directory found. Skipping design-gate check."
  exit 0
fi

has_failure=0

for change_dir in "$changes_root"/*; do
  [[ -d "$change_dir" ]] || continue

  change_name="$(basename "$change_dir")"
  [[ "$change_name" == "archive" ]] && continue

  proposal="$change_dir/proposal.md"
  design="$change_dir/design.md"
  tasks="$change_dir/tasks.md"

  [[ -f "$proposal" ]] || continue

  # Heuristic: treat as UI-affecting when proposal or design references UI/frontend/portal scope.
  if ! grep -Eiq 'ui-affecting|frontend|portal|management-portal|web-portal' "$proposal" && \
     ! { [[ -f "$design" ]] && grep -Eiq 'ui-affecting|frontend|portal|management-portal|web-portal' "$design"; }; then
    continue
  fi

  echo "Checking OpenSpec design gate for UI-affecting change: $change_name"

  if ! grep -q 'DESIGN.md' "$proposal"; then
    echo "ERROR: $change_name/proposal.md must reference affected portal DESIGN.md guides."
    has_failure=1
  fi

  if [[ ! -f "$design" ]] || ! grep -q 'DESIGN.md' "$design"; then
    echo "ERROR: $change_name/design.md must reference affected portal DESIGN.md guides."
    has_failure=1
  fi

  if [[ ! -f "$tasks" ]] || ! grep -Eiq 'pre-apply gate.*DESIGN\.md|DESIGN\.md.*pre-apply gate' "$tasks"; then
    echo "ERROR: $change_name/tasks.md must include a pre-apply design gate task referencing DESIGN.md."
    has_failure=1
  fi
done

if [[ "$has_failure" -ne 0 ]]; then
  echo "OpenSpec design-gate check failed."
  exit 1
fi

echo "OpenSpec design-gate check passed."
