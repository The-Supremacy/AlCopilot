#!/bin/bash
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')
RESULT_TYPE=$(echo "$INPUT" | jq -r '.toolResult.resultType')
TIMESTAMP=$(echo "$INPUT" | jq -r '.timestamp')

mkdir -p logs

jq -n \
  --arg ts "$TIMESTAMP" \
  --arg tool "$TOOL_NAME" \
  --arg result "$RESULT_TYPE" \
  --arg args "$TOOL_ARGS" \
  '{timestamp: $ts, tool: $tool, result: $result, args: $args}' >> logs/copilot-audit.jsonl
