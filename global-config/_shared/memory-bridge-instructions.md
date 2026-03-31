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

## When to Use Memory

Use memory only for durable cross-session context:

- architecture decisions and rationale
- team conventions and recurring patterns
- project-specific facts that should persist

Do not persist:

- secrets, credentials, tokens
- ephemeral debugging notes
- session-only chatter

## Token-Efficient Defaults

1. Do not auto-recall at session start.
2. Recall only when task needs prior context.
3. Use specific queries with small `maxResults`.
4. Persist only high-value outcomes after meaningful decisions.
5. If memory is unavailable, continue normally without blocking.
