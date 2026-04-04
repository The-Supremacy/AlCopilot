#!/bin/bash
set -e

INPUT=$(cat)
SOURCE=$(echo "$INPUT" | jq -r '.source')
TIMESTAMP=$(echo "$INPUT" | jq -r '.timestamp')
PROMPT=$(echo "$INPUT" | jq -r '.initialPrompt // "none"')

mkdir -p logs

echo "{\"event\":\"session_start\",\"source\":\"$SOURCE\",\"timestamp\":$TIMESTAMP,\"prompt\":$(echo "$PROMPT" | jq -Rs .)}" >> logs/copilot-sessions.jsonl
