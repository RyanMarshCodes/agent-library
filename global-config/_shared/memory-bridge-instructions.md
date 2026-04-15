# Ryan.MCP Knowledge-Graph Memory

Canonical memory usage for all AI tools in this setup.

Current backend: Ryan.MCP native Postgres memory store (persistent docker volume via AppHost).

## Primary Tool Path (Preferred)

Use Ryan.MCP top-level tools first:

- `memory_status` to check availability
- `memory_recall(query, maxResults=5)` for targeted recall
- `memory_persist(entityName, entityType, observations)` to save durable learnings
- `memory_link(fromEntity, toEntity, relationType)` to connect entities
- `memory_read()` only for full-graph review (expensive)

## Recall Query Best Practices

`memory_recall` is intentionally lightweight and works best with focused query terms.

- Prefer short targeted queries over long natural-language sentences.
- Use exact entity names/slugs when possible (for example `test-entity-final`).
- If a long query returns empty, retry with:
  - key phrase only
  - hyphenated/sluggified form
  - core noun terms (2-5 words)

Examples:

- Good: `memory_recall(query="test-entity-final")`
- Good: `memory_recall(query="auth-service decision")`
- Weak: `memory_recall(query="please find the thing we discussed about auth migration last week")`

## Fallback Tool Path (Low-Level Debug)

If top-level memory tools are unavailable or need debugging, use the connector bridge:

1. `list_external_mcp_tools` with `connector: "memory"`
2. `call_external_mcp_tool` with:
   - `connector: "memory"`
   - `tool`: `search_nodes`, `read_graph`, `create_entities`, `add_observations`, `create_relations`, etc.
   - `argumentsJson`: JSON object string matching downstream schema

Note: the legacy external `memory` connector may be disabled when native Postgres memory is active.

Important for bridge calls:

- `argumentsJson` must be a JSON object **string** (not null/undefined).
- For `search_nodes`, pass at least `{"query":"..."}`.
- If you pass null or omit required args, downstream tools often error with "expects object".

Working bridge example:

```json
{
  "connector": "memory",
  "tool": "search_nodes",
  "argumentsJson": "{\"query\":\"test-entity-final\"}"
}
```

## Proactive Memory Workflow

Memory usage should be **automatic and contextual** — not dependent on the user asking for it. But it must also be **token-conscious** — every recall burns input tokens, every persist burns output tokens.

**Who owns memory?** The host tool (Claude Code, Cursor, Gemini, OpenCode) and the orchestrator agent handle recall/persist. Individual specialist agents do **not** call memory tools directly — they receive relevant context via their delegation briefs and report outcomes back to the orchestrator.

### When to Recall (do this without being asked)

- **Starting a non-trivial task in a known domain**: One focused query, `maxResults=3`. Example: `memory_recall(query="auth-service")` before touching auth code.
- **Encountering a design decision**: Check if a prior decision exists before proposing a new approach.
- **Debugging a recurring issue**: Check if it's been seen before.
- **Skip recall** for trivial tasks (typo fixes, single imports, formatting) and for tasks with no plausible prior context.

### When to Persist (do this without being asked)

- **Architecture or design decisions** with rationale
- **Non-obvious conventions** discovered or established
- **Tricky bug root causes** that could recur
- **New reusable patterns** introduced
- **Skip persist** for trivial changes, routine work, or anything already in memory.

## Token-Efficient Defaults

1. Recall only when task context would benefit — not reflexively at session start.
2. Use specific queries with small `maxResults` (3-5, not 10).
3. Persist only high-value outcomes — decisions, conventions, patterns, resolutions.
4. If memory is unavailable, continue normally without blocking.
5. Prefer `memory_recall(query, maxResults=3)` over `memory_read()` — the latter dumps the entire graph.

## Dynamic MCP Invocation Policy

- Treat MCP tools as on-demand capabilities, not default steps.
- Prefer local workspace context first (files, symbols, tests) before MCP calls.
- Invoke MCP only when it provides unique value for the current task.
- If a task is trivial or fully answerable from local context, skip MCP entirely.
- For memory workflows, use at most one focused recall query before expanding scope.
