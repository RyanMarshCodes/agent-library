# Ryan.MCP Reference

Setup and tool reference for the local MCP server. This file is NOT loaded into every session — it's a reference document. See `AGENTS.md` for the trigger table and usage rules that ARE loaded per-session.

## Starting the MCP Server

```bash
cd mcp-server
dotnet run --project src/Ryan.MCP.AppHost --launch-profile http
```

## MCP Configuration

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

## Tool Reference

| Tool | Purpose |
|------|---------|
| **Context** | |
| `get_context(language, task?)` | Polyglot entry point — surfaces relevant agents + standards for any stack |
| **Agents** | |
| `recommend_agent(task)` | Returns the best-matching agent with activation instructions |
| `list_agents(scope?)` | Browse all indexed agents |
| `get_agent(name)` | Fetch full agent instructions |
| `search_agents(query)` | Keyword search across agents |
| `list_agent_scopes` | List available agent categories and counts |
| `agent_status` | Check agent ingestion status |
| `ingest_agents` | Re-scan and re-index all agent files |
| **Standards & Documents** | |
| `list_standards(tier?, language?)` | Browse indexed standards documents |
| `read_document(tier, path)` | Fetch full content of a standards document |
| `search_documents(query)` | Search across all standards content |
| `ingestion_status` | Check document ingestion status |
| `ingest_documents` | Re-scan and re-index all documents |
| `parse_openapi(tier, path)` | Parse an OpenAPI/Swagger spec and return structured summary |
| **Memory** | |
| `memory_status` | Check memory store availability |
| `memory_recall(query, maxResults?)` | Targeted recall from the knowledge graph |
| `memory_persist(entityName, entityType, observations)` | Save durable learnings to the knowledge graph |
| `memory_link(fromEntity, toEntity, relationType)` | Connect related entities in the graph |
| `memory_read` | Read entire knowledge graph (expensive — prefer `memory_recall`) |
| **Model Mapping** | |
| `recommend_model(task?)` | Recommend best model for a task or agent |
| `get_model_mapping(agentName)` | Get stored model mapping for a specific agent |
| `list_model_mappings(tier?)` | List all agent-model mappings |
| `update_model_mapping(agentName, primaryModel, tier)` | Manually set a model mapping |
| `sync_model_mappings` | Re-sync all mappings from agent frontmatter |
| `list_llm_providers` | List configured LLM providers and available models |
| **Build & PR** | |
| `fix_build(workingDirectory?)` | Iteratively fix a broken .NET build |
| `generate_pr_description(workingDirectory?)` | Generate a structured PR description from git diff |
| `nuget_hygiene(workingDirectory?)` | Check for vulnerable/outdated NuGet packages |
| **Log Forensics** | |
| `analyze_runtime_logs(path)` | Analyze log files for top error patterns and root-cause clues |
| `summarize_errors(path, groupBy?)` | Summarize errors grouped by signature or level |
| `incident_timeline(path, start, end)` | Create a time-ordered incident timeline from a log file |
| **Connectors & Utilities** | |
| `list_external_connectors` | Show configured external MCP connectors (URLs, enabled flag) |
| `list_external_mcp_tools(connector)` | List downstream tools for an enabled connector (e.g. `memory`) |
| `call_external_mcp_tool(connector, tool, argumentsJson?)` | Invoke a downstream MCP tool; `argumentsJson` is a JSON object string |
| `fetch(url)` | Fetch a URL and return clean text content |

**Memory connector** (`connector` = `memory`) — typical tools: `create_entities`, `create_relations`, `add_observations`, `delete_entities`, `delete_observations`, `delete_relations`, `read_graph`, `search_nodes`, `open_nodes`.

## Standards Precedence

1. `knowledge/global/` — official language/framework standards (official tier)
2. `knowledge/backend/` — backend standards (organization tier)
3. `knowledge/frontend/` — frontend standards (project tier)

When rules conflict, the most specific rule wins.
