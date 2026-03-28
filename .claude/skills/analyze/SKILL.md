---
name: analyze
description: Senior architect-level analysis of the current project — stack detection, architecture, quality, risks
argument-hint: "[path or focus area — optional, defaults to current directory]"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/code-analysis.agent.md` then follow the workflow defined in that file exactly.

Target: $ARGUMENTS

Current directory: !`pwd`
Project files (top level): !`ls -la 2>/dev/null || dir`
Git status: !`git log --oneline -5 2>/dev/null || echo "Not a git repository"`
