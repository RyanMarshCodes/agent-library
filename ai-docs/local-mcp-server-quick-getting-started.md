# Ryan.LocalDev MCP Server - Quick Getting Started

## TL;DR

1. **Prerequisites**: .NET 10 SDK, Docker Desktop

2. **Clone/Scaffold**: The project is in `mcp-local/` folder

3. **Run**:
```powershell
cd mcp-local
dotnet run --project src/Ryan.LocalDev.AppHost
```

4. **Access**: http://localhost:8787

5. **Test**:
```powershell
curl http://localhost:8787/mcp/health_check
curl http://localhost:8787/mcp/external_connectors
```

---

## Prerequisites

Install these once:

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0+ | Building and running |
| Docker Desktop | Latest | Container orchestration |

Verify installation:
```powershell
dotnet --version
docker --version
```

---

## Project Structure

```
mcp-local/
├── src/
│   ├── Ryan.LocalDev.AppHost/     # Aspire orchestrator
│   └── Ryan.LocalDev.Mcp/         # MCP Server (.NET)
├── data/
│   └── docs/
│       ├── official/             # Official standards
│       ├── org/                  # Organization standards
│       └── project/              # Project-specific standards
├── README.md
└── .env.example
```

---

## Running the MCP Server

### Start

```powershell
cd mcp-local
dotnet run --project src/Ryan.LocalDev.AppHost
```

### Expected Output

```
info: Aspire.Hosting.DistributedApplication[0]
      Application host directory is: ...
info: Aspire.Hosting.DistributedApplication[0]
      Distributed application starting.
...
```

### Check It's Running

```powershell
# Health check
curl http://localhost:8787/mcp/health_check

# List external connectors (SequentialThinking should be enabled)
curl http://localhost:8787/mcp/external_connectors

# Ingestion status
curl http://localhost:8787/mcp/ingestion_status
```

---

## Available Connectors

### Enabled by Default

| Service | Port | Description |
|---------|------|-------------|
| Ryan.LocalDev MCP | 8787 | Main MCP server |
| SequentialThinking | 8788 | Reasoning/analysis |

### Disabled by Default (Enable in appsettings.json)

| Service | Port | Description |
|---------|------|-------------|
| Azure MCP | 8790 | Azure resource management |
| Docker MCP | 8791 | Container management |
| Discord MCP | 8792 | Discord bot notifications |
| Filesystem MCP | 8793 | Cross-project file access |
| Ollama | 11434 | Local LLM |
| Qdrant | 6333 | Vector store |

---

## Enable Additional Connectors

Edit `src/Ryan.LocalDev.AppHost/appsettings.json`:

```json
{
  "Projects": {
    "SequentialThinking": { "Enabled": "true" },
    "Azure": { "Enabled": "true" },
    "Docker": { "Enabled": "false" },
    "Discord": { "Enabled": "false" },
    "Filesystem": { "Enabled": "false" },
    "Ollama": { "Enabled": "false" },
    "Qdrant": { "Enabled": "false" }
  }
}
```

---

## Environment Variables (Optional)

Copy `.env.example` to `.env` and configure:

### Azure MCP
```env
AZURE_AUTH_MODE=cli
# Or service principal:
AZURE_TENANT_ID=<tenant-id>
AZURE_CLIENT_ID=<client-id>
AZURE_CLIENT_SECRET=<secret>
```

### Discord MCP
```env
DISCORD_BOT_TOKEN=<your-bot-token>
```

---

## IDE Integration

### VS Code / Claude Desktop / OpenCode

Add to your MCP configuration:

```json
{
  "mcpServers": {
    "ryan-localdev": {
      "url": "http://localhost:8787/mcp"
    }
  }
}
```

---

## Available Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Health check |
| `/mcp/health_check` | GET | MCP health + connectors |
| `/mcp/list_standards` | GET | List available standards |
| `/mcp/ingest_documents` | POST | Trigger ingestion |
| `/mcp/ingestion_status` | GET | Ingestion status |
| `/mcp/external_connectors` | GET | Connector status |

---

## Troubleshooting

### Port Already in Use
```powershell
# Find what's using the port
netstat -ano | findstr 8787
```

### Docker Not Running
- Start Docker Desktop and wait for it to fully start

### Can't Connect
- Verify MCP is running: `curl http://localhost:8787/health`
- Check firewall settings

---

## For Scaffolding a New Instance

To scaffold a new MCP server project like this:

1. Create new folder
2. Run:
```powershell
dotnet new sln -n YourProjectName
dotnet new aspire-apphost -n YourProjectName.AppHost
dotnet new web -n YourProjectName.Mcp
dotnet sln add src/YourProjectName.AppHost/YourProjectName.AppHost.csproj
dotnet sln add src/YourProjectName.Mcp/YourProjectName.Mcp.csproj
```

3. Copy the MCP implementation files from this project

4. Configure AppHost with connectors as needed
