# Skills Plan

**Date**: 2026-03-27
**Status**: Planning — pending review before implementation

Skills are Claude Code slash commands (`.claude/skills/<name>/SKILL.md`). They are Claude Code-specific but wrap the portable agent instruction files in `ai-docs/agents/`, so the logic stays universal.

---

## Portability Strategy

```
ai-docs/agents/*.agent.md   ← canonical, portable Markdown (any AI)
       ↓ deployed to
.claude/agents/*.md          ← Claude Code subagent auto-delegation
.claude/skills/*/SKILL.md    ← Claude Code slash commands
```

Skills reference agent files by telling Claude to read and follow them. The skills themselves are thin launchers — the real logic lives in the agent docs.

---

## Proposed Skills

### Tier 1: Project Lifecycle (highest value for personal projects)

| Skill | Command | Agent Used | Description |
|---|---|---|---|
| `new-project` | `/new-project [stack] [name]` | CodeAnalysisAgent + DocumentationAgent | Scaffold a new project in any stack — git init, folder structure, tooling config, README |
| `analyze` | `/analyze [path?]` | CodeAnalysisAgent | Full architect-level analysis of current or specified project |
| `troubleshoot` | `/troubleshoot [issue or paste]` | TroubleshootingAgent | Root cause analysis from a description, error, or stack trace |
| `document` | `/document [type?] [path?]` | DocumentationAgent | Generate or update documentation (README, API, ADR, runbook, etc.) |
| `tech-debt` | `/tech-debt [scope?]` | TechDebtAnalysisAgent | Evidence-based tech debt register and remediation proposal |

### Tier 2: Code Quality

| Skill | Command | Agent Used | Description |
|---|---|---|---|
| `security-audit` | `/security-audit [path?]` | SecurityCheckAgent | OWASP-aligned security audit for any stack |
| `simplify` | `/simplify [file]` | CodeSimplifierAgent | Clean and simplify a file without changing behavior |
| `review` | `/review [file or diff?]` | CodeAnalysisAgent + CodeSimplifierAgent | Senior code review: correctness, quality, security, style |

### Tier 3: Agent Management

| Skill | Command | Agent Used | Description |
|---|---|---|---|
| `new-agent` | `/new-agent [description]` | AIAgentExpert | Design and write a new agent instruction file |
| `audit-agents` | `/audit-agents` | AIAgentExpert | Audit all agents in the library for gaps, overlap, hardcoding |

### Tier 4: Stack-Specific Helpers (modular, add as needed)

These are thin skills that set stack context before handing off to a core agent. The core agents are already stack-agnostic, so these just pre-seed context.

| Skill | Command | Stacks | Description |
|---|---|---|---|
| `init-ts` | `/init-ts [name]` | TypeScript/Node/Bun | Scaffold a TypeScript project (tsconfig, eslint, prettier, vitest) |
| `init-dotnet` | `/init-dotnet [name] [template]` | C#/.NET | Scaffold a .NET project (solution, web API, tests, Aspire) |
| `init-python` | `/init-python [name]` | Python | Scaffold a Python project (venv, pyproject.toml, ruff, pytest) |
| `init-go` | `/init-go [name]` | Go | Scaffold a Go module (go.mod, main.go, Makefile, tests) |
| `init-react` | `/init-react [name]` | React/Vite | Scaffold a React app (Vite, TypeScript, Tailwind, React Router) |

---

## What Each Skill Looks Like

Each skill is a thin wrapper that:
1. Reads the relevant agent instruction file
2. Sets context (arguments, current project path, detected stack)
3. Tells Claude to follow that agent's workflow

**Example — `/troubleshoot`:**
```yaml
---
name: troubleshoot
description: Root cause analysis from a bug description, error message, or stack trace
argument-hint: "[issue description or paste error here]"
allowed-tools: Read, Grep, Glob, Bash
---

Read and follow the agent instructions at:
D:\Projects\agent-configurations\ai-docs\agents\troubleshooting.agent.md

Issue to investigate:
$ARGUMENTS

Current project: !`pwd`
Recent git changes: !`git log --oneline -10 2>/dev/null || echo "Not a git repo"`
```

---

## Implementation Order

1. **Tier 1** — `/new-project`, `/analyze`, `/troubleshoot`, `/document`, `/tech-debt`
2. **Tier 2** — `/security-audit`, `/simplify`, `/review`
3. **Tier 3** — `/new-agent`, `/audit-agents`
4. **Tier 4** — Stack init skills as needed

Also needed alongside skills:
- Deploy agents to `.claude/agents/` so Claude Code can auto-delegate without a slash command
- Add a language standards file per stack (TypeScript, Python, Go, Java, React) under `backend/` or `frontend/`

---

## Open Questions for Review

1. **Skill scope**: global (`~/.claude/skills/`) vs. repo-local (`.claude/skills/`)? Global makes sense since these are personal project helpers.
2. **`/new-project` detail**: how opinionated should the scaffolding be? (e.g., always Vitest for TS, or let user specify test framework?)
3. **Stack init skills**: start with just TS + .NET (your primary stacks), or include Python/Go/React from day one?
4. **Path references**: skills will hard-reference `ai-docs/agents/` — is the path always `D:\Projects\agent-configurations\` or should this be an env var / relative?
