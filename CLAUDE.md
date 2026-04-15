
Follow `AGENTS.md` as the canonical, global standards for this workspace.

## Canonical AI/Agent Workflows

All Claude agents must support the baseline workflows defined in:

 mcp-server/knowledgebase/WORKFLOW_COMMANDS.md


## Karpathy-Inspired Behavioral Guidelines
See [knowledge/global/karpathy-guidelines.md](knowledge/global/karpathy-guidelines.md) for the canonical guidelines. These apply to all Claude agents and should be merged with project-specific instructions.
These workflows include /spec, /validate, /architect, /implement, /test, /reflect, /review, /commit, and /context/init. Surface these as slash commands, recipes, or prompt patterns wherever possible. See AGENTS.md for invocation and implementation details.

## Slash Commands

| Command | Description |
|---------|-------------|
| `/dotnet-pr` | Generate PR description: 5-bullet summary, risks + rollback, paste-ready description, reviewer focus |
| `/feature-workflow` | Run spec-based feature delivery flow (/context -> /spec -> /validate -> /architect -> /implement -> /test -> /reflect -> /review -> /commit) |
| `/proceed` | Run end-to-end ADLC-compatible pipeline with validation gates |
| `/wrapup` | Finalize feature artifacts and capture durable knowledge |
| `/status` | Show current workflow phase and recommended next command |
| `/bugfix` | Run streamlined bug workflow (report -> analyze -> fix -> verify) |

- Prefer the most specific rules available (nested `AGENTS.md` / folder rules).
- If project/repo-specific rules conflict with `AGENTS.md`, follow the project/repo rules.
- Confirm before destructive actions (deletions, large refactors/renames, migrations, force pushes).
- Verify changes with the most relevant fast checks (format/lint/unit tests) when available.
- Use `docs/` for long-term memory: read at session start, write a session summary at session end (see `docs/sessions/_template.md`).
