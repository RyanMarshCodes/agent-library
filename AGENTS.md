# Development Standards

!include "D:/Projects/agent-configurations/global-config/_shared/context-management.md"

This file defines global, language-agnostic development standards for all code generated in this project.

## Scope & precedence

- These rules apply everywhere unless a nested `AGENTS.md` (or folder-specific rules) provides more specific guidance for a language/framework/tooling.
- When local rules conflict with this file, **follow the more specific local rules**.
- When using these standards inside another project/repo, **that repo’s project-specific rules override this file** (e.g., its own `AGENTS.md`, `.cursor/rules`, `CLAUDE.md`, `GEMINI.md`, `.github/copilot-instructions.md`, etc.).
- If a rule here is impractical in a specific change, document the exception (and rationale) in the PR/issue/commit message and keep the deviation as small as possible.

## AI agent workflow (prompt/context efficiency & safety)

- Start with a brief plan: goal, constraints, and 2-5 next steps.
- Gather only the context you need: read relevant files, prefer scoped searches, avoid dragging in unrelated code.
- Delegate in parallel when helpful: use subagents for exploration, running commands/tests, or reviewing changes; merge results back into one coherent solution.
- Confirm before destructive actions: ask first for file deletions, large refactors/renames, broad rewrites, schema migrations, force pushes, or security-sensitive changes.
- Keep changes small and reviewable: prefer incremental diffs, avoid drive-by rewrites, and preserve existing public APIs unless explicitly requested.
- Always verify: run the most relevant fast checks (format/lint/unit tests) and fix any issues introduced; don’t ignore failing checks.
- Communicate decisions and trade-offs: explain “why” succinctly; call out assumptions and how to validate them.
- Protect secrets and safety: never paste keys/tokens; avoid logging sensitive data; prefer environment variables and redaction.
- **Detect shell before first command**: on first shell/terminal use in a session, identify the active shell — run `$PSVersionTable` (succeeds → PowerShell) or check `$SHELL`/`$0` (bash/zsh). Cache the result and use shell-appropriate syntax for the rest of the session. Never assume; never retry with a different syntax after failure — detect first.

### Shell syntax reference

| Shell | Detection | Path sep | Null device | Env var |
|-------|-----------|----------|-------------|---------|
| PowerShell | `$PSVersionTable` succeeds | `\` or `/` | `$null` / `NUL` | `$env:VAR` |
| bash / zsh / Git Bash | `echo $SHELL` returns path | `/` | `/dev/null` | `$VAR` |
| CMD | neither above works | `\` | `NUL` | `%VAR%` |

### Orchestration rule

- **Atomic tasks** (single domain, single deliverable): handle directly — do not route through the orchestrator.
- **Compound tasks** (two or more distinct domains, or clearly requiring multiple specialist agents): invoke `OrchestratorAgent` (`agents/orchestrator.agent.md`). It will decompose, route, parallelise, and aggregate.
- **Unclear scope**: use `OrchestratorAgent` to decompose before acting.

When in doubt whether a task is compound: if completing it would naturally require switching mental models more than once (e.g. security + implementation + documentation), it’s compound.

### Token discipline (apply in every response)

- No preamble. Never open with “Sure!”, “Great question”, or any acknowledgement. Start with action or answer.
- No narration. Do not describe what you are about to do — do it.
- No re-explanation. Do not repeat back what the user said.
- Results only. Report outcomes and decisions, not process.
- One question max. If ambiguous, ask the single most important question. If a reasonable assumption can be made, make it and note it in the result.
- Batch parallel work. Never serialise independent subtasks.

## Communication Style

- Keep responses short and direct — avoid introductions, sign-offs, and unnecessary explanations
- Answer the question or take the action immediately; don't preface with "Sure, I can help"
- Don't ask for confirmation on trivial, reversible actions — just do them
- Admit mistakes directly and fix them rather than continuing down the wrong path
- Only explain "why" when it's non-obvious or requested

---

## Long-term memory (`docs/`)

Every project/repo should maintain its own `docs/` folder as a local, personal log. This is **not** shared via version control — `docs/` should be added to the project's `.gitignore` (or `~/.gitignore_global`). It exists only on your machine, scoped to that repo.

This central config repo (`agent-configurations`) maintains its own `docs/` only for sessions that work on the config repo itself. Do not log sessions from other projects here.

### Setting up `docs/` in a new project
If a project doesn't have a `docs/` folder yet, create one using the structure defined in this repo:
- Copy `docs/sessions/_template.md` as a starting point for session summaries
- Create `docs/issues-resolutions.md` for the issues log
- Add `docs/` to `.gitignore`

### At the start of every session
1. Read `docs/sessions/_template.md` to understand the session summary format.
2. Read `docs/issues-resolutions.md` for known problems and how they were handled.
3. Scan recent files in `docs/sessions/` for relevant prior context.

### At the end of every session
1. Append any new issues and their resolutions to `docs/issues-resolutions.md`.
2. Write a session summary to `docs/sessions/YYYY-MM-DD-{agent}.md` (e.g. `2026-03-17-claude.md`). If a file for that date and agent already exists, append to it rather than overwrite.
3. Follow the required session summary structure in `docs/sessions/_template.md`.

### What belongs in `docs/`
- Decisions made and their rationale
- Non-obvious issues encountered and how they were resolved
- Patterns observed across the codebase worth remembering
- Open items or follow-ups for future sessions

### What does NOT belong in `docs/`
- Code or configuration (those live in the codebase)
- Things already obvious from reading the source
- Ephemeral debugging notes with no lasting value
- Secrets, credentials, or tokens (never)

---

## Testing

- All new/changed production code should be covered by automated tests appropriate to the stack (unit/integration/e2e)
- Target >90% coverage for new/changed code where coverage is measurable; prefer meaningful assertions over chasing metrics
- Write meaningful tests that verify behavior, not just coverage metrics
- Test edge cases, error conditions, and happy paths
- Use mocking/faking appropriately to isolate units under test

---

## Code Quality

- Avoid introducing compiler warnings or linter warnings; fix any you introduce
- Avoid introducing new code smells; opportunistically clean up low-effort, non-breaking smells in nearby code
- Run formatters and linters when available before committing
- Avoid commented-out code; if needed temporarily, use a `TODO`/`FIXME` with a tracking issue/link

---

## Security

- Never expose secrets, keys, credentials, or tokens in code
- Never log secrets or sensitive data
- Validate untrusted inputs (user input, API responses, file contents) and apply the correct boundary protection (encoding/sanitization) for the sink (HTML/SQL/OS/filesystem, etc.)
- Use parameterized queries / prepared statements for all database operations
- Follow OWASP guidelines for web applications
- Use environment variables for configuration secrets
- Implement proper authentication and authorization checks

---

## Architecture & Design Principles

- **SOLID**: Apply all five principles
  - **S**ingle Responsibility: One class/function does one thing
  - **O**pen/Closed: Open for extension, closed for modification
  - **L**iskov Substitution: Subtypes must be substitutable for base types
  - **I**nterface Segregation: Prefer small, specific interfaces
  - **D**ependency Inversion: Depend on abstractions, not concretions

- **CLEAN Architecture**: Keep layers loosely coupled with clear boundaries
- **DRY**: Don't Repeat Yourself - extract common patterns into shared utilities
- **KISS**: Keep solutions simple and pragmatic
- **SRP**: Every module, class, and function should have one reason to change

---

## Readability & Maintainability

- Write self-documenting code with clear, descriptive names
- Keep functions small and focused (ideally <20 lines)
- Keep classes focused on a single responsibility
- Use explicit error handling - never fail silently
- Handle errors at the appropriate layer
- Use appropriate data structures for the task

---

## Code Style

- Follow **latest/modern** language and framework official documentation best practices:
  - **C#/.NET**: Microsoft official conventions → see `knowledge/backend/code-standards/csharp/`
  - **React**: React Team recommendations → see `knowledge/frontend/react/` (when present)
  - **Angular**: Angular style guide → see `knowledge/frontend/angular/` (when present)
  - **Other**: Community-accepted conventions for the language/framework
- Use consistent formatting (enforce with formatters)
- Use meaningful variable and function names
- Comment the "why", not the "what"

---

## Version Control

- Make atomic, focused commits
- Write clear commit messages describing the "why"
- Run the full test suite and linters (when available) before pushing
- Use branches for feature work
- Squash or rebase before merging to keep history clean

---

## General Best Practices

- Use type systems effectively (static typing preferred when available)
- Handle edge cases explicitly
- Fail fast with clear error messages
- Log appropriately - useful for debugging, never sensitive data
- Write documentation for public APIs and complex logic
- Review code before submitting - verify it meets these standards

---

## Local MCP Server

This project includes a local MCP server at `mcp-server/` (Ryan.MCP) that centralises all agents, standards documents, and external MCP connectors.

### Starting the MCP Server

```bash
cd mcp-server
dotnet run --project src/Ryan.MCP.AppHost --launch-profile http
```

### MCP Configuration

**Claude Code** (one-time setup):
```bash
claude mcp add --transport http --scope global ryan-mcp http://localhost:8787/mcp
```

**Other IDEs** (Cursor, Rider, VS Code, etc.) — add to your MCP config:
```json
{
  "mcpServers": {
    "ryan-mcp": {
      "type": "http",
      "url": "http://localhost:8787/mcp"
    }
  }
}
```

### Proactive MCP usage (non-negotiable triggers)

When `ryan-mcp` is connected, **do not start reasoning from training data alone**. Call the appropriate tool first — these are not suggestions:

| Situation | Tool to call |
|-----------|-------------|
| Starting any non-trivial coding task | `get_context(language, task)` |
| Unsure which specialist agent to use | `recommend_agent(task)` |
| Working in a stack you haven't seen in this session | `get_context(language)` |
| Looking for a coding standard or convention | `search_documents(query)` |
| Recalling a past architectural decision or convention | `call_external_mcp_tool` with connector `memory`, tool `search_nodes`, arguments `{"query":"..."}` |
| Recording a new decision, convention, or pattern worth remembering | `call_external_mcp_tool` with connector `memory`, tools `create_entities` / `add_observations` / `create_relations` as needed |

**`get_context` is the default entry point.** It returns the right agents and standards for your stack in one call. Call it before writing code, not after.

**Knowledge-graph memory** lives on the external `memory` connector (`@modelcontextprotocol/server-memory` behind Aspire when `Projects:Memory:Enabled` is true). Ryan.MCP does **not** expose those tool names at the top level; invoke them through **`call_external_mcp_tool`**. Use `list_external_mcp_tools` with `connector: "memory"` to confirm names and schemas against your running server. This is separate from `docs/` (file-based session memory); the graph is queryable and relational.

### Fallback (MCP not running or not connected)

Fall back to the agent library at `agents/` and standards at `knowledge/global/`, `knowledge/backend/`, and `knowledge/frontend/` directly. Use `recommend_agent`-style reasoning manually: read the catalog at `catalog.json`, find the best-matching agent, load it.

### MCP tool reference

| Tool | Purpose |
|------|---------|
| `get_context(language, task?)` | Polyglot entry point — surfaces relevant agents + standards for any stack |
| `recommend_agent(task)` | Returns the best-matching agent with activation instructions |
| `list_agents(scope?)` | Browse all indexed agents |
| `get_agent(name)` | Fetch full agent instructions |
| `search_agents(query)` | Keyword search across agents |
| `list_standards(tier?, language?)` | Browse indexed standards documents |
| `read_document(tier, path)` | Fetch full content of a standards document |
| `search_documents(query)` | Search across all standards content |
| `list_external_connectors` | Show configured external MCP connectors (URLs, enabled flag) |
| `list_external_mcp_tools(connector)` | List downstream tools for an enabled connector (e.g. `memory`) |
| `call_external_mcp_tool(connector, tool, argumentsJson?)` | Invoke a downstream MCP tool; `argumentsJson` is a JSON object string |

**Memory connector (`connector` = `memory`)** — tools from `@modelcontextprotocol/server-memory` (typical names): `create_entities`, `create_relations`, `add_observations`, `delete_entities`, `delete_observations`, `delete_relations`, `read_graph`, `search_nodes`, `open_nodes`. Example: `call_external_mcp_tool` with `connector` `memory`, `tool` `search_nodes`, `argumentsJson` `{"query":"authentication"}`.

### Standards Precedence

The MCP server applies standards in this order:
1. `knowledge/global/` — official language/framework standards (official tier)
2. `knowledge/backend/` — backend standards (organization tier)
3. `knowledge/frontend/` — frontend standards (project tier)

When rules conflict, the most specific rule wins.
