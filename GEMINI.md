Follow `AGENTS.md` as the canonical, global standards for this workspace.

## Canonical AI/Agent Workflows

Use the ADLC-compatible workflow conventions in:

	mcp-server/knowledgebase/WORKFLOW_COMMANDS.md

## Karpathy-Inspired Behavioral Guidelines
See [knowledge/global/karpathy-guidelines.md](knowledge/global/karpathy-guidelines.md) for the canonical guidelines. These apply to all Gemini sessions and should be merged with project-specific instructions.
Primary feature pipeline:

	/context -> /spec -> /validate -> /architect -> /validate -> /implement -> /test -> /reflect -> /review -> /commit -> /wrapup

- Prefer the most specific rules available (nested `AGENTS.md` / folder rules).
- If project/repo-specific rules conflict with `AGENTS.md`, follow the project/repo rules.
- Confirm before destructive actions (deletions, large refactors/renames, migrations, force pushes).
- Verify changes with the most relevant fast checks (format/lint/unit tests) when available.
- Use `docs/` for long-term memory: read at session start, write a session summary at session end (see `docs/sessions/_template.md`).
