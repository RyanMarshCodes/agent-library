---
name: troubleshoot
description: Root cause analysis for any bug, error, exception, or stack trace — any language or framework
argument-hint: "[describe the issue, paste the error message, or paste the full stack trace]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/troubleshooting.agent.md` then follow the workflow defined in that file exactly.

Issue to investigate:
$ARGUMENTS

Current directory: !`pwd`
Recent git changes: !`git log --oneline -10 2>/dev/null || echo "Not a git repository"`
