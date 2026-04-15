# Development Standards

## Karpathy-Inspired Behavioral Guidelines (applies to all agents)

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

### 1. Think Before Coding
**Don't assume. Don't hide confusion. Surface tradeoffs.**
Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First
**Minimum code that solves the problem. Nothing speculative.**
- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.
Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes
**Touch only what you must. Clean up only your own mess.**
When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.
When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.
The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution
**Define success criteria. Loop until verified.**
Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"
For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```
Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---
**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

!include "D:/Projects/agent-configurations/global-config/_shared/context-management.md"

## Canonical AI/Agent Workflows

All agents and tools (Copilot, Claude, Cursor, OpenCode, Antigravity, etc.) must support the baseline workflows defined in:

  mcp-server/knowledgebase/WORKFLOW_COMMANDS.md

These workflows include /spec, /validate, /architect, /implement, /test, /reflect, /review, /commit, /wrapup, /proceed, /status, /bugfix, and /context/init. They are to be surfaced as slash commands, recipes, or prompt patterns wherever possible.

### Workflow Invocation

- Users may invoke workflows by slash command (e.g., /spec, /review) or by natural prompt (e.g., "Let's build a new feature: ...").
- Agents should route these requests to the appropriate skill or workflow as described in the canonical doc.
- For discoverability, agents should expose a /workflows or /commands endpoint or help command listing all available workflows.

### Agent Implementation

- All new agent .md files must reference this section for workflow conventions.

## Scope & precedence

- These rules apply everywhere unless a nested `AGENTS.md` (or folder-specific rules) provides more specific guidance for a language/framework/tooling.
- When local rules conflict with this file, **follow the more specific local rules**.
- When using these standards inside another project/repo, **that repo's project-specific rules override this file** (e.g., its own `AGENTS.md`, `.cursor/rules`, `CLAUDE.md`, `GEMINI.md`, `.github/copilot-instructions.md`, etc.).
- If a rule here is impractical in a specific change, document the exception (and rationale) in the PR/issue/commit message and keep the deviation as small as possible.

## AI agent workflow (prompt/context efficiency & safety)

- Start with a brief plan: goal, constraints, and 2-5 next steps.
- Gather only the context you need: read relevant files, prefer scoped searches, avoid dragging in unrelated code.
- Delegate in parallel when helpful: use subagents for exploration, running commands/tests, or reviewing changes; merge results back into one coherent solution.
- Confirm before destructive actions: ask first for file deletions, large refactors/renames, broad rewrites, schema migrations, force pushes, or security-sensitive changes.
- Keep changes small and reviewable: prefer incremental diffs, avoid drive-by rewrites, and preserve existing public APIs unless explicitly requested.
- Always verify: run the most relevant fast checks (format/lint/unit tests) and fix any issues introduced; don't ignore failing checks.
- Communicate decisions and trade-offs: explain "why" succinctly; call out assumptions and how to validate them.
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

When in doubt whether a task is compound: if completing it would naturally require switching mental models more than once (e.g. security + implementation + documentation), it's compound.

### Token discipline (apply in every response)

- No preamble. Never open with "Sure!", "Great question", or any acknowledgement. Start with action or answer.
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
  - **React**: React Team recommendations → see `knowledge/frontend/frameworks/react/` (when present)
  - **Angular**: Angular style guide → see `knowledge/frontend/frameworks/angular/` (when present)
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

This project includes a local MCP server at `mcp-server/` (Ryan.MCP) that centralises all agents, standards documents, and external MCP connectors. For setup instructions and the full tool reference, see `docs/mcp-reference.md`.

### Proactive MCP Usage & Knowledge-graph Memory

All workflow, tool usage, and convention for memory (knowledge graph) access is governed exclusively by the canonical instructions in [`global-config/_shared/memory-bridge-instructions.md`](global-config/_shared/memory-bridge-instructions.md). Do not duplicate, summarize, or paraphrase its content in this or any other file. Instead, refer directly to that document for details, triggers, patterns, examples, and fallback rules.

- **Summary:** Use the documented memory tools for context recall and persistence as described in the canonical source. Never duplicate the process description. Any agent or user-facing documentation should point only to the canonical memory-bridge-instructions.

**If Ryan.MCP is not running:** fall back to agent/standards discovery using `agents/` and standards at `knowledge/global/`, `knowledge/backend/`, `knowledge/frontend/` directly. Use `catalog.json` to find the best-matching agent.
