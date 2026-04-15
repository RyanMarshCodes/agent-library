---
name: feature-workflow
description: ADLC-compatible new feature workflow with explicit validation gates
argument-hint: "Feature request or user story"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the ADLC-compatible pipeline from `D:/Projects/agent-configurations/mcp-server/knowledgebase/WORKFLOW_COMMANDS.md`.

Input: $ARGUMENTS

Execution order:
1. /context
2. /spec
3. /validate (spec)
4. /architect
5. /validate (architecture/tasks)
6. /implement
7. /test
8. /reflect
9. /review
10. /commit
11. /wrapup

Rules:
- Do not advance a gate while blocker findings exist.
- Re-run /validate after blocker fixes.
- Keep diffs small and verifiable.
- Use MCP tools when available: list_workflows, get_workflow, start_feature_workflow, run_workflow_step.
