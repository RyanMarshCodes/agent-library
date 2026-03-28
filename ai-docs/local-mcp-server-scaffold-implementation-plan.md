# Local MCP Server - Scaffolding Plan

## Goal

Create a reusable MCP server project for personal/side projects, orchestrated by .NET Aspire AppHost, with support for external MCP connectors.

## Success Criteria

1. .NET Aspire AppHost orchestrating a .NET MCP server
2. MCP server exposes HTTP endpoints for tools
3. Document ingestion with file watching
4. Standards precedence: official → org → project
5. External MCP connectors support (SequentialThinking, Azure, Docker, etc.)
6. Local-only, no deployment required

## Project Naming

Ask the user for:
- **Solution name**: e.g., `YourName.LocalDev`
- **AppHost project name**: e.g., `{Name}.AppHost`
- **MCP Server project name**: e.g., `{Name}.Mcp`

## Recommended Stack

1. **MCP Server Runtime**: .NET 10 (ASP.NET Core)
2. **Orchestration**: .NET Aspire AppHost
3. **Language**: C#

## Scaffold Commands

Replace `{SolutionName}` with your desired name:

```powershell
# Create folder and solution
mkdir {SolutionName}
cd {SolutionName}
dotnet new sln -n {SolutionName}

# Create AppHost
dotnet new aspire-apphost -n {SolutionName}.AppHost -o src/{SolutionName}.AppHost

# Create MCP Server
dotnet new web -n {SolutionName}.Mcp -o src/{SolutionName}.Mcp

# Add to solution
dotnet sln add src/{SolutionName}.AppHost/{SolutionName}.AppHost.csproj
dotnet sln add src/{SolutionName}.Mcp/{SolutionName}.Mcp.csproj

# Add reference
cd src/{SolutionName}.AppHost
dotnet add reference ../{SolutionName}.Mcp/{SolutionName}.Mcp.csproj
```

## Project Structure

```
src/
├── {Name}.AppHost/
│   ├── AppHost.cs              # Aspire orchestration
│   ├── appsettings.json        # Project toggles
│   └── {Name}.AppHost.csproj
└── {Name}.Mcp/
    ├── Program.cs              # HTTP endpoints
    ├── Configuration/
    │   └── McpOptions.cs       # Config classes
    ├── Services/
    │   ├── DocumentIngestionCoordinator.cs
    │   └── ExternalConnectorRegistry.cs
    ├── appsettings.json
    └── {Name}.Mcp.csproj
```

## Required NuGet Packages

None required - uses built-in ASP.NET Core and Aspire.Hosting.

## Configuration

### AppHost appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aspire.Hosting.Dcp": "Warning"
    }
  },
  "Projects": {
    "SequentialThinking": { "Enabled": "true" },
    "Azure": { "Enabled": "false" },
    "Docker": { "Enabled": "false" },
    "Discord": { "Enabled": "false" },
    "Filesystem": { "Enabled": "false" },
    "Ollama": { "Enabled": "false" },
    "Qdrant": { "Enabled": "false" }
  }
}
```

### MCP Server appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "McpOptions": {
    "Knowledge": {
      "ProjectSlug": "personal-localdev",
      "OfficialPath": "../../data/docs/official",
      "OrganizationPath": "../../data/docs/org",
      "ProjectPath": "../../data/docs/project",
      "IndexPath": "../../data/index"
    },
    "Ingestion": {
      "AutoIngestOnStartup": true,
      "WatchForChanges": true,
      "DebounceMilliseconds": 1500,
      "AllowedExtensions": [ ".md", ".txt", ".json", ".yaml", ".yml" ]
    },
    "ExternalConnectors": [
      {
        "Name": "sequential-thinking",
        "Enabled": true,
        "Transport": "http",
        "Endpoint": "http://localhost:8788",
        "TimeoutMs": 30000,
        "Headers": {}
      }
    ],
    "ExternalConnectorEndpointOverrides": {}
  }
}
```

## AppHost.cs Template

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var docsPath = Path.Combine(builder.AppHostDirectory, "..", "..", "data", "docs");
var indexPath = Path.Combine(builder.AppHostDirectory, "..", "..", "data", "index");

var mcpServer = builder.AddProject<Projects.{Name}_Mcp>("mcp-server")
    .WithHttpEndpoint(port: 8787, targetPort: 8787)
    .WithEnvironment("ASPNETCORE_URLS", "http://127.0.0.1:8787")
    .WithEnvironment("McpOptions__Knowledge__ProjectSlug", "personal-localdev")
    .WithEnvironment("McpOptions__Knowledge__OfficialPath", Path.Combine(docsPath, "official"))
    .WithEnvironment("McpOptions__Knowledge__OrganizationPath", Path.Combine(docsPath, "org"))
    .WithEnvironment("McpOptions__Knowledge__ProjectPath", Path.Combine(docsPath, "project"))
    .WithEnvironment("McpOptions__Knowledge__IndexPath", indexPath);

// SequentialThinking - Enabled by default
var sequentialThinkingEnabled = builder.Configuration["Projects:SequentialThinking:Enabled"]?.ToLower() != "false";
if (sequentialThinkingEnabled)
{
    var sequentialThinking = builder.AddContainer("sequentialthinking", "mcp/sequentialthinking:latest")
        .WithEndpoint(name: "mcp", port: 8788, targetPort: 8788);
    
    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__sequential-thinking", 
        sequentialThinking.GetEndpoint("mcp"));
}

// Azure MCP - Disabled by default
var azureEnabled = builder.Configuration["Projects:Azure:Enabled"]?.ToLower() == "true";
if (azureEnabled)
{
    var azureMcp = builder.AddContainer("azure-mcp", "mcr.microsoft.com/azure-app-configuration/mcp-server:latest")
        .WithEndpoint(name: "mcp", port: 8790, targetPort: 8790);
    
    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__azure", 
        azureMcp.GetEndpoint("mcp"));
}

// Docker MCP - Disabled by default
var dockerEnabled = builder.Configuration["Projects:Docker:Enabled"]?.ToLower() == "true";
if (dockerEnabled)
{
    var dockerMcp = builder.AddContainer("docker-mcp", "mcp/server-docker:latest")
        .WithEndpoint(name: "mcp", port: 8791, targetPort: 8791)
        .WithBindMount(source: "/var/run/docker.sock", target: "/var/run/docker.sock", isReadOnly: true);
    
    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__docker", 
        dockerMcp.GetEndpoint("mcp"));
}

// Discord MCP - Disabled by default
var discordEnabled = builder.Configuration["Projects:Discord:Enabled"]?.ToLower() == "true";
if (discordEnabled)
{
    var discordMcp = builder.AddContainer("discord-mcp", "ghcr.io/saseq/discord-mcp:latest")
        .WithEndpoint(name: "mcp", port: 8792, targetPort: 8792)
        .WithEnvironment("DISCORD_BOT_TOKEN", Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? "");
    
    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__discord", 
        discordMcp.GetEndpoint("mcp"));
}

// Filesystem MCP - Disabled by default
var filesystemEnabled = builder.Configuration["Projects:Filesystem:Enabled"]?.ToLower() == "true";
if (filesystemEnabled)
{
    var filesystemMcp = builder.AddContainer("filesystem-mcp", "mcp/server-filesystem:latest")
        .WithEndpoint(name: "mcp", port: 8793, targetPort: 8793)
        .WithBindMount(source: Path.Combine(builder.AppHostDirectory, "..", "..", "data"), target: "/data", isReadOnly: true);
    
    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__filesystem", 
        filesystemMcp.GetEndpoint("mcp"));
}

// Ollama - Disabled by default
var ollamaEnabled = builder.Configuration["Projects:Ollama:Enabled"]?.ToLower() == "true";
if (ollamaEnabled)
{
    var ollama = builder.AddContainer("ollama", "ollama/ollama:latest")
        .WithHttpEndpoint(port: 11434, targetPort: 11434);
}

// Qdrant - Disabled by default
var qdrantEnabled = builder.Configuration["Projects:Qdrant:Enabled"]?.ToLower() == "true";
if (qdrantEnabled)
{
    var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant:latest")
        .WithHttpEndpoint(port: 6333, targetPort: 6333);
}

builder.Build().Run();
```

## HTTP Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Health check |
| `/mcp/health_check` | GET | MCP health + connectors |
| `/mcp/list_standards` | GET | List available standards |
| `/mcp/ingest_documents` | POST | Trigger document ingestion |
| `/mcp/ingestion_status` | GET | Ingestion status |
| `/mcp/external_connectors` | GET | Connector status |

## Running

```powershell
dotnet run --project src/{Name}.AppHost
```

## IDE Integration

```json
{
  "mcpServers": {
    "localdev": {
      "url": "http://localhost:8787/mcp"
    }
  }
}
```

## Available Connectors

| Connector | Port | Default | Description |
|-----------|------|---------|-------------|
| MCP Server | 8787 | Enabled | Main server |
| SequentialThinking | 8788 | Enabled | Reasoning |
| Azure MCP | 8790 | Disabled | Azure resources |
| Docker MCP | 8791 | Disabled | Container mgmt |
| Discord MCP | 8792 | Disabled | Notifications |
| Filesystem MCP | 8793 | Disabled | File access |
| Ollama | 11434 | Disabled | Local LLM |
| Qdrant | 6333 | Disabled | Vector store |

## Document Folders

Place standards documents in:
- `data/docs/official/` - Official standards
- `data/docs/org/` - Organization standards
- `data/docs/project/` - Project standards

## Paste-Ready Prompt

Use this in a fresh session, replacing `{SolutionName}` with your desired name:

```
Create a new .NET MCP server project named {SolutionName} with:
1. Solution: {SolutionName}
2. AppHost project: {SolutionName}.AppHost (Aspire)
3. MCP Server project: {SolutionName}.Mcp (ASP.NET Core web)
4. Add ExternalConnectorRegistry and DocumentIngestionCoordinator services
5. Configure SequentialThinking connector enabled by default
6. Add AppHost configuration for Azure, Docker, Discord, Filesystem, Ollama, Qdrant connectors (disabled by default)
7. Use HTTP transport on port 8787
8. Include document ingestion with file watching
9. Include standards precedence (official → org → project)
```

## What This Produces

- ✅ .NET Aspire AppHost orchestration
- ✅ .NET MCP Server with REST endpoints
- ✅ Document auto-ingestion with file watching
- ✅ External MCP connector support
- ✅ SequentialThinking enabled by default
- ✅ Standards knowledge base structure
