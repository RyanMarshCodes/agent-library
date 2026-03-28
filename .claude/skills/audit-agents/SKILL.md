---
name: audit-agents
description: Audit all agents in the library for gaps, overlap, hardcoded values, missing guardrails, and cross-platform compatibility
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/ai-agent-expert.agent.md` then follow the auditing workflow defined in that file.

Agent library path: `D:/Projects/agent-configurations/ai-docs/agents/`

All agent files to audit: !`ls D:/Projects/agent-configurations/ai-docs/agents/`
Catalog: !`cat D:/Projects/agent-configurations/ai-docs/catalog.json`

Read each agent file. Produce a full audit report covering:
- Purpose clarity (is the single responsibility clear?)
- Platform portability (any hardcoded org/project names?)
- Guardrail completeness (destructive/remote actions covered?)
- Delegation coverage (does each agent delegate to the right specialists?)
- Naming consistency (files: `kebab-case.agent.md`; catalog `name`: stable PascalCase id; H1: human-readable title)
- catalog.json sync (all files registered? all entries have corresponding files?)

Write the audit report to `D:/Projects/agent-configurations/ai-docs/agent-audit-{date}.md` where {date} is today's date in YYYY-MM-DD format.
