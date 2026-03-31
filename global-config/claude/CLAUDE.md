# Global instructions (Claude Code)

## Ryan.MCP knowledge-graph memory (MCP bridge)

When **ryan-mcp** is connected and the task needs **cross-session** persistent memory (architecture decisions, team conventions, recurring patterns, durable facts—not only this conversation):

1. **Discover** downstream tool names: call **`list_external_mcp_tools`** with **`connector`**: `"memory"`. If the connector is disabled or the call fails, say so and continue without the graph.
2. **Recall**: **`call_external_mcp_tool`** with `connector` `"memory"`, `tool` `"search_nodes"`, `argumentsJson` a JSON object string such as `{"query":"your topic"}`.
3. **Persist**: invoke **`create_entities`**, **`add_observations`**, **`create_relations`** (and deletes when appropriate) through **`call_external_mcp_tool`** with the same `connector` and a JSON **`argumentsJson`** matching the tool schema.
4. **Do not** assume top-level MCP tools named `search_nodes`, `create_entities`, etc. exist on the host. They are **only** reachable via **`call_external_mcp_tool`** unless you have added a separate MCP server entry for memory.

**Typical memory tools** (from `@modelcontextprotocol/server-memory`): `read_graph`, `search_nodes`, `open_nodes`, `create_entities`, `create_relations`, `add_observations`, `delete_entities`, `delete_observations`, `delete_relations`.

**Example:** `call_external_mcp_tool` → `connector`: `memory`, `tool`: `search_nodes`, `argumentsJson`: `{"query":"authentication"}`.
