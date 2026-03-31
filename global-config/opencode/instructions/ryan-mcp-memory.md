# Knowledge-graph memory via **ryan-mcp** (single MCP connection)

When **ryan-mcp** is connected, **do not require a separate Memory MCP**. The knowledge graph is reached through **ryan-mcp** only:

1. **`list_external_mcp_tools`** with **`connector`**: `"memory"` — confirms the downstream tool names (optional when you already know them).
2. **`call_external_mcp_tool`** with:
   - **`connector`**: `"memory"`
   - **`tool`**: e.g. `search_nodes`, `read_graph`, `create_entities`, `add_observations`, `create_relations`, `open_nodes`, or the delete tools
   - **`argumentsJson`**: a JSON **object** as a string, e.g. `{"query":"topic"}` or `{"entities":[...]}` per the tool schema.

Downstream tools are **not** registered as top-level OpenCode tools; they only run through **`call_external_mcp_tool`**.

## Session start (automatic)

At the **start of each session**, automatically call `search_nodes` with a broad query (e.g., `""` or `"session context"`) to retrieve all existing memory. Use `read_graph` as an alternative to get the full graph. Present a brief summary of what was found (if anything).

## Proactive behavior (not automatic)

Instructions do not run tools for you—**call** them when useful.

For non-trivial work, **recall first** (`search_nodes` via the bridge), then **persist** (`create_entities` / `add_observations` / `create_relations`) when the user would want it next session. If `list_external_mcp_tools` or `call_external_mcp_tool` fails (connector disabled or MCP down), say so briefly and continue.

**Typical `tool` names**: `read_graph`, `search_nodes`, `open_nodes`, `create_entities`, `create_relations`, `add_observations`, `delete_entities`, `delete_observations`, `delete_relations`.
