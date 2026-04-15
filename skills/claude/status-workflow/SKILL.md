---
name: status-workflow
description: Show current ADLC-compatible workflow status and recommended next command
argument-hint: "Optional scope (feature, branch, or phase)"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Report workflow status for:
$ARGUMENTS

Include:
- Current phase
- Completed gates
- Blockers and warnings
- Recommended next command
