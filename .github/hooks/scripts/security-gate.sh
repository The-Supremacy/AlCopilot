#!/bin/bash
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')

# --- Block destructive commands ---
if [ "$TOOL_NAME" = "bash" ]; then
  COMMAND=$(echo "$TOOL_ARGS" | jq -r '.command // empty')

  if echo "$COMMAND" | grep -qE "rm -rf|rm -r |sudo |DROP TABLE|DROP DATABASE|force push|--force|--no-verify|mkfs|chmod -R 777"; then
    echo '{"permissionDecision":"deny","permissionDecisionReason":"Destructive command blocked by security hook"}'
    exit 0
  fi
fi

# --- Block edits to protected files ---
if [ "$TOOL_NAME" = "edit" ] || [ "$TOOL_NAME" = "create" ]; then
  FILE_PATH=$(echo "$TOOL_ARGS" | jq -r '.path // .filePath // empty')

  # Architecture docs — human-only
  if echo "$FILE_PATH" | grep -qE "docs/architecture\.md"; then
    echo '{"permissionDecision":"deny","permissionDecisionReason":"docs/architecture.md is protected — human-only edits"}'
    exit 0
  fi

  # Lock files
  if echo "$FILE_PATH" | grep -qE "(\.lock|lock\.json|lock\.yaml|lock\.yml)$"; then
    echo '{"permissionDecision":"deny","permissionDecisionReason":"Lock files must not be edited directly"}'
    exit 0
  fi
fi
