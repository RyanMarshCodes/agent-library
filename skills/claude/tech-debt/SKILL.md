---
name: tech-debt
description: Evidence-based technical debt analysis with a structured register and prioritized remediation proposal
argument-hint: "[scope or focus area — optional, defaults to full project]"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Use the Read tool to read `D:/Projects/agent-configurations/agents/tech-debt-analysis.agent.md` then follow the workflow defined in that file exactly.

Scope: $ARGUMENTS

Current directory: !`pwd`
Project files (top level): !`ls -la 2>/dev/null || dir`
Git log: !`git log --oneline -10 2>/dev/null || echo "Not a git repository"`
