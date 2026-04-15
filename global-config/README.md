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

# Deploy only (skip regeneration)
.\sync-and-deploy.ps1 -DeployOnly

# Force overwrite existing files
.\sync-and-deploy.ps1 -Force
```

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
