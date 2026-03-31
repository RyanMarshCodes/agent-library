# Global Copilot Instructions

Follow the standards and conventions defined in the project's `AGENTS.md` if present.
If no project-specific `AGENTS.md` exists, apply the principles below.

## Code quality

- >90% test coverage. Zero tolerance for warnings and code smells.
- Follow SOLID, CLEAN, DRY, KISS, SRP.
- No hardcoded secrets. Validate all inputs at system boundaries only.

## Style

- Match the language/framework conventions already present in the project.
- Prefer explicit over implicit. Prefer simple over clever.
- Write code that reads like a clear explanation of intent.

## Ryan.MCP knowledge-graph memory (MCP bridge)

When **ryan-mcp** is connected and the task needs **cross-session** persistent memory:

1. **Discover**: `list_external_mcp_tools` with `connector: "memory"`.
2. **Recall**: `call_external_mcp_tool` → connector `"memory"`, tool `"search_nodes"`, argumentsJson `{"query":"topic"}`.
3. **Persist**: use `create_entities`, `add_observations`, `create_relations` via `call_external_mcp_tool`.
