# Global Configurations

Centralized configuration for AI coding tools. Changes made through tool UIs/settings automatically sync here via symlinks/junctions.

## Architecture

```
global-config/              # Source of truth (git-tracked)
├── _shared/               # Shared content blocks
│   ├── mcp-servers.toml   # MCP server definitions (single source)
│   └── *.md               # Shared instructions
├── claude/                # Claude Code
├── cursor/                # Cursor IDE
├── gemini/                # Gemini CLI / Antigravity
├── opencode/              # OpenCode
├── copilot/               # GitHub Copilot CLI
└── vscode/                # VS Code
```

## Tool Configuration Map

| Tool | Config Location | Symlinked To |
|------|-----------------|--------------|
| Claude Code | `~/.claude/` | `claude/` |
| Cursor | `%APPDATA%\Cursor\User\` | `cursor/` |
| Gemini CLI | `~/.gemini/` | `gemini/` |
| OpenCode | `~/.config/opencode/` | `opencode/` |
| Copilot CLI | `~/.copilot/` | `copilot/` |
| VS Code | `~/.vscode/` | `vscode/` |

## MCP Servers

MCP server configurations are centralized in `_shared/mcp-servers.toml`. Each tool generates its own config format from this source:

| Tool | Format |
|------|--------|
| Claude Code | `mcpServers` (HTTP: `url`) |
| Cursor | `mcpServers` (HTTP: `url`, stdio: `command`/`args`) |
| Gemini CLI | `mcpServers` (HTTP: `httpUrl`) |
| OpenCode | `mcp` (HTTP: `url`, stdio: `command`/`args`) |
| Copilot CLI | `mcpServers` (HTTP: `type: http`, stdio: `type: local`) |
| VS Code | `servers` (stdio only) |

### Ryan.MCP (Central Brain)

Ryan.MCP handles the "knowledge graph" (memory, project context) centrally. It runs as an HTTP server.

### Per-Tool STDIO Servers

STDIO-based MCP servers (Playwright, Chrome DevTools, Aspire) are configured directly in each tool. This avoids needing an MCP bridge/proxy for local stdio servers.

## Deployment

### Prerequisites

**Windows**: Enable Developer Mode for symlinks to work without elevation:

- Settings → System → For Developers → Developer Mode: **On**

### Commands

```powershell
# Full sync: generate configs + deploy symlinks
cd global-config
.\sync-and-deploy.ps1

# Generate only (preview what would change)
.\sync-and-deploy.ps1 -GenerateOnly

# Generate with full MCP server set (default is lean)
.\sync-and-deploy.ps1 -GenerateOnly -McpProfile full

# Deploy only (skip regeneration)
.\sync-and-deploy.ps1 -DeployOnly

# Force overwrite existing files
.\sync-and-deploy.ps1 -Force
```

### MCP Profiles

`sync-and-deploy.ps1` supports MCP profiles to keep tool usage dynamic:

- `lean` (default): includes only `ryan-mcp` and `filesystem`
- `full`: includes all servers from `_shared/mcp-servers.toml`

Use `-McpProfile full` only when you need browser automation or other non-default MCP capabilities.

## Adding New Tools

1. Add tool folder under `global-config/`
2. Add shared blocks to `_shared/` if applicable
3. Update `sync-and-deploy.ps1`:
   - `Invoke-Generate`: Add config generation
   - `Invoke-Deploy`: Add symlink creation
4. Run `.\sync-and-deploy.ps1 -GenerateOnly` to test

## Notes

- **Junctions** (directory symlinks) work without elevation on Windows
- **File symlinks** require Developer Mode or administrator privileges
- Configs are regenerated from shared blocks on every run
- Direct edits to tool config folders work (changes sync to repo automatically)

## Observability (OTEL → Aspire)

Telemetry from all agents flows to the Aspire dashboard via OpenTelemetry.

### Ryan.MCP

Ryan.MCP emits structured logs, traces, and metrics via Serilog + OTEL:

- **Structured logs**: Serilog with scope enrichers (OperationId, ToolName, etc.)
- **Traces**: ASP.NET Core instrumentation + HTTP client spans
- **Metrics**: Request counts, durations, custom Ryan.MCP meters
- **Export**: gRPC OTLP to `OTEL_EXPORTER_OTLP_ENDPOINT` (default: `http://localhost:4317`)

**Note**: Check your Aspire dashboard Settings → Telemetry for the actual OTEL collector endpoint.

### Agent Tools (ILogger Scopes)

All MCP tools in Ryan.MCP use structured logging with scope enrichment:

```csharp
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["ToolName"] = "memory_recall",
    ["Query"] = query,
    ["MaxResults"] = maxResults
});

_logger.LogInformation("Starting memory recall");
```

This automatically attaches `ToolName`, `Query`, `MaxResults` to all logs within the scope.

### Other Agents (Claude Code, OpenCode, etc.)

Set OTEL env vars to export to Aspire dashboard:

```bash
# Add to ~/.bashrc, ~/.zshrc, or your shell profile
source ~/path/to/global-config/_shared/observability-env.sh

# Or manually:
export CLAUDE_CODE_ENABLE_TELEMETRY=1
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
export OTEL_EXPORTER_OTLP_PROTOCOL=grpc
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp
```

### Aspire Dashboard

View all telemetry in the Aspire dashboard:

- **Traces**: Distributed request traces across services
- **Metrics**: Custom meters, request rates, latencies
- **Logs**: Structured logs with scope-enriched attributes

Start Aspire: `cd mcp-server && dotnet run --project src/Ryan.MCP.AppHost`
