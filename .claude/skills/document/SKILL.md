---
name: document
description: Generate or update technical documentation for any project — README, API docs, ADR, runbook, onboarding guide, changelog
argument-hint: "[type: readme|api|adr|runbook|onboarding|changelog — optional] [path — optional]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/documentation.agent.md` then follow the workflow defined in that file exactly.

Request: $ARGUMENTS

Current directory: !`pwd`
Project files (top level): !`ls -la 2>/dev/null || dir`
