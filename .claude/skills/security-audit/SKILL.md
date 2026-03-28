---
name: security-audit
description: OWASP Top 10 security audit for any codebase, stack, or domain — produces categorized findings with remediation
argument-hint: "[path or component to audit — optional, defaults to full project]"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/security-check.agent.md` then follow the workflow defined in that file exactly.

Audit target: $ARGUMENTS

Current directory: !`pwd`
Project files (top level): !`ls -la 2>/dev/null || dir`
