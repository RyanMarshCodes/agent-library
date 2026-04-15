Follow `AGENTS.md` as the canonical, global standards for this workspace.

## Canonical AI/Agent Workflows

All Copilot sessions should use the workflow conventions in:

	mcp-server/knowledgebase/WORKFLOW_COMMANDS.md

For new feature development, prefer the ADLC-compatible spec-based flow:

	/context -> /spec -> /validate -> /architect -> /validate -> /implement -> /test -> /reflect -> /review -> /commit -> /wrapup

Natural-language trigger is also valid:

	Let's build a new feature: <description>

- Prefer the most specific rules available (nested `AGENTS.md` / folder rules).
- If project/repo-specific rules conflict with `AGENTS.md`, follow the project/repo rules.
- Confirm before destructive actions (deletions, large refactors/renames, migrations, force pushes).
- Verify changes with the most relevant fast checks (format/lint/unit tests) when available.
- Use `docs/` for long-term memory: read at session start, write a session summary at session end (see `docs/sessions/_template.md`).
