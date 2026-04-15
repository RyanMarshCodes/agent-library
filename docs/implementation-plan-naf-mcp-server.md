# NAF MCP Server Implementation Plan

## Overview

This plan scaffolds a centralized MCP server for NAF developers using .NET Aspire. It provides:

- Agent library with SDLC workflows
- Persistent memory (Postgres)
- Canonical NAF tech knowledgebase (Postgres + pgvector)
- Standards documents
- Secure-by-default operations and upgrade process
- All accessible via GitHub Copilot in VS Code

**Not using this repo**: This is a standalone plan for building a new NAF MCP server from scratch.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                 NAF MCP Server                      │
├─────────────────────────────────────────────────────┤
│  .NET Aspire AppHost                               │
│  ├── Mcp project (exposes tools)                   │
│  ├── Postgres (memory + knowledge graph + vectors) │
│  └── (optional) other resources                    │
└─────────────────────────────────────────────────────┘
           ▲
           │ MCP protocol
           ▼
┌─────────────────────────────────────────────────────┐
│            NAF Developer VS Code                   │
│  .vscode/mcp.json → connects to NAF MCP server    │
└─────────────────────────────────────────────────────┘
```

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET | 9.0+ | Runtime |
| Aspire CLI | Latest | Local orchestration |
| PostgreSQL | 15+ | Memory + knowledgebase storage |
| pgvector extension | Latest | Vector similarity search |
| Docker Desktop | Latest | Container runtime |

---

## Phase 1: Project Scaffolding

### Step 1.1: Scaffold with Aspire workload/template (preferred)

```bash
# Install Aspire workload/templates (one-time setup)
dotnet workload install aspire

# Inspect available Aspire templates in your SDK
dotnet new list aspire

# Create solution + AppHost from Aspire templates
dotnet new sln -n NafMcp
dotnet new aspire-apphost -n NafMcp.AppHost -o src/NafMcp.AppHost
dotnet new web -n NafMcp.Mcp -o src/NafMcp.Mcp
dotnet sln NafMcp.sln add src/NafMcp.AppHost src/NafMcp.Mcp
```

If you prefer, you can scaffold the same structure via the Aspire CLI instead of manual `dotnet new` commands.

### Step 1.2: Verify scaffolded Aspire dependencies (add only if missing)

If you scaffolded with Aspire templates/CLI, most Aspire references are preconfigured. Confirm the following baseline packages exist before continuing:

In `NafMcp.AppHost.csproj`:

```xml
<PackageReference Include="Aspire.Hosting" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.Postgres" Version="9.0.0" />
```

In `NafMcp.Mcp.csproj`:

```xml
<PackageReference Include="Aspire.StackExchange.Redis" Version="9.0.0" />
```

### Step 1.3: Configure AppHost

In `src/NafMcp.AppHost/Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("naf-postgres", port: 5432)
    .WithDataBindMount();

var mcp = builder.AddProject<NafMcp.Mcp.Project>("naf-mcp")
    .WithReference(postgres);

builder.Build().Run();
```

---

## Phase 2: MCP Server Implementation

### Step 2.1: Core MCP server setup

In `NafMcp.Mcp/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .FromHandler<MemoryToolHandler>()
    .FromHandler<AgentsToolHandler>()
    .FromHandler<DocumentsToolHandler>()
    .FromHandler<KnowledgebaseToolHandler>();

var app = builder.Build();
app.MapMcpSandboxEndpoints();
app.Run();
```

### Step 2.2: Add packages for MCP

In `NafMcp.Mcp.csproj`:

```xml
<PackageReference Include="Aspire.StackExchange.Redis.OutputCache" Version="9.0.0" />
<PackageReference Include="Npgsql" Version="9.0.0" />
<PackageReference Include="Pgvector" Version="0.2.2" />
<PackageReference Include="Dapper" Version="2.1.0" />
```

---

## Phase 3: Tools Implementation

### Step 3.1: Memory Tools

Create `src/NafMcp.Mcp/Tools/MemoryTools.cs`:

```csharp
public class MemoryTools
{
    private readonly NpgsqlConnection _db;

    public MemoryTools(NpgsqlConnection db) => _db = db;

    [McpTool("memory_recall")]
    public async Task<string> RecallAsync(
        [McpToolParam(Documentation = "Query for recall")] string query,
        [McpToolParam(Documentation = "Max results")] int maxResults = 5)
    {
        // Search memory for relevant context
        var sql = @"
            SELECT entity_name, entity_type, observations, created_at
            FROM memory.Entities
            WHERE to_tsvector('english', entity_name || ' ' || array_to_string(observations, ' ')) @@ plainto_tsquery('english', @query)
            ORDER BY created_at DESC
            LIMIT @maxResults";
        
        return await _db.QueryAsync<MemoryResult>(sql, new { query, maxResults });
    }

    [McpTool("memory_persist")]
    public async Task PersistAsync(
        [McpToolParam(Documentation = "Entity name")] string entityName,
        [McpToolParam(Documentation = "Entity type")] string entityType,
        [McpToolParam(Documentation = "Observations to store")] List<string> observations)
    {
        var sql = @"
            INSERT INTO memory.Entities (entity_name, entity_type, observations, created_at)
            VALUES (@entityName, @entityType, @observations, NOW())
            ON CONFLICT (entity_name) DO UPDATE SET observations = EXCLUDED.observations";
        
        await _db.ExecuteAsync(sql, new { entityName, entityType, observations });
    }

    [McpTool("memory_status")]
    public Task<MemoryStatus> StatusAsync()
    {
        // Return memory status
    }
}
```

### Step 3.2: Agent Tools

Create `src/NafMcp.Mcp/Tools/AgentTools.cs`:

```csharp
public class AgentTools
{
    private readonly AgentCatalog _catalog;

    public AgentTools(AgentCatalog catalog) => _catalog = catalog;

    [McpTool("list_agents")]
    public Task<List<AgentInfo>> ListAsync(
        [McpToolParam(Documentation = "Optional scope filter")] string? scope = null)
    {
        // List available agents, optionally filtered
    }

    [McpTool("get_agent")]
    public Task<AgentDefinition> GetAsync(
        [McpToolParam(Documentation = "Agent name")] string agentName)
    {
        // Get specific agent definition
    }

    [McpTool("recommend_agent")]
    public Task<AgentRecommendation> RecommendAsync(
        [McpToolParam(Documentation = "Task description")] string task)
    {
        // Recommend best agent for task
    }
}
```

### Step 3.3: Document Tools

Create `src/NafMcp.Mcp/Tools/DocumentTools.cs`:

```csharp
public class DocumentTools
{
    private readonly DocumentIndex _index;

    [McpTool("list_standards")]
    public Task<List<StandardDoc>> ListStandardsAsync(
        [McpToolParam(Documentation = "Language filter")] string? language = null)
    {
        // List standards documents
    }

    [McpTool("search_documents")]
    public Task<List<SearchResult>> SearchAsync(
        [McpToolParam(Documentation = "Search query")] string query)
    {
        // Search standards documents
    }

    [McpTool("read_document")]
    public Task<string> ReadAsync(
        [McpToolParam(Documentation = "Document tier")] string tier,
        [McpToolParam(Documentation = "Document path")] string path)
    {
        // Read document content
    }
}
```

### Step 3.4: Knowledgebase Tools (graph + vector retrieval)

Create `src/NafMcp.Mcp/Tools/KnowledgeTools.cs`:

```csharp
public class KnowledgeTools
{
    private readonly KnowledgebaseService _knowledgebase;

    public KnowledgeTools(KnowledgebaseService knowledgebase) => _knowledgebase = knowledgebase;

    [McpTool("kb_search")]
    public Task<List<KbSearchResult>> SearchAsync(
        [McpToolParam(Documentation = "Search query")] string query,
        [McpToolParam(Documentation = "Optional product filter")] string? product = null,
        [McpToolParam(Documentation = "Optional domain filter")] string? domain = null,
        [McpToolParam(Documentation = "Max results")] int maxResults = 8)
    {
        // Hybrid retrieval: keyword + vector similarity + graph expansion
    }

    [McpTool("kb_get_entity")]
    public Task<KbEntity> GetEntityAsync(
        [McpToolParam(Documentation = "Entity id or name")] string entity)
    {
        // Get canonical product/system/workflow entity
    }

    [McpTool("kb_get_workflow")]
    public Task<KbWorkflow> GetWorkflowAsync(
        [McpToolParam(Documentation = "Workflow name")] string workflowName)
    {
        // Return workflow steps, systems involved, and links to source docs
    }

    [McpTool("kb_refresh_index")]
    public Task<RefreshResult> RefreshIndexAsync(
        [McpToolParam(Documentation = "Optional source path")] string? source = null)
    {
        // Re-ingest documents and regenerate embeddings
    }
}
```

---

## Phase 4: Schema Setup

### Step 4.1: Initialize Postgres Schema

Create `src/NafMcp.Mcp/schema.sql`:

```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE SCHEMA IF NOT EXISTS memory;
CREATE SCHEMA IF NOT EXISTS kb;

CREATE TABLE IF NOT EXISTS memory.Entities (
    id SERIAL PRIMARY KEY,
    entity_name VARCHAR(255) NOT NULL UNIQUE,
    entity_type VARCHAR(100) NOT NULL,
    observations TEXT[] NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS memory.Relations (
    id SERIAL PRIMARY KEY,
    from_entity VARCHAR(255) NOT NULL,
    relation_type VARCHAR(100) NOT NULL,
    to_entity VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (from_entity) REFERENCES memory.Entities(entity_name),
    FOREIGN KEY (to_entity) REFERENCES memory.Entities(entity_name)
);

CREATE INDEX idx_entities_name ON memory.Entities(entity_name);
CREATE INDEX idx_entities_type ON memory.Entities(entity_type);
CREATE INDEX idx_relations_from ON memory.Relations(from_entity);
CREATE INDEX idx_relations_to ON memory.Relations(to_entity);

CREATE TABLE IF NOT EXISTS kb.Entities (
    id UUID PRIMARY KEY,
    entity_name VARCHAR(255) NOT NULL UNIQUE,
    entity_kind VARCHAR(100) NOT NULL, -- product, service, workflow, team, domain
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    source_ref TEXT NOT NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS kb.Relations (
    id UUID PRIMARY KEY,
    from_entity_id UUID NOT NULL REFERENCES kb.Entities(id),
    relation_type VARCHAR(100) NOT NULL, -- depends_on, owns, calls, supports, part_of
    to_entity_id UUID NOT NULL REFERENCES kb.Entities(id),
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (from_entity_id, relation_type, to_entity_id)
);

CREATE TABLE IF NOT EXISTS kb.Documents (
    id UUID PRIMARY KEY,
    source_path TEXT NOT NULL UNIQUE,
    source_kind VARCHAR(100) NOT NULL, -- standards, runbook, architecture, workflow
    title TEXT NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS kb.DocumentChunks (
    id UUID PRIMARY KEY,
    document_id UUID NOT NULL REFERENCES kb.Documents(id) ON DELETE CASCADE,
    chunk_index INT NOT NULL,
    content TEXT NOT NULL,
    content_tsv tsvector,
    embedding vector(1536),
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (document_id, chunk_index)
);

CREATE INDEX idx_kb_entities_kind ON kb.Entities(entity_kind);
CREATE INDEX idx_kb_relations_from ON kb.Relations(from_entity_id);
CREATE INDEX idx_kb_relations_to ON kb.Relations(to_entity_id);
CREATE INDEX idx_kb_docs_kind ON kb.Documents(source_kind);
CREATE INDEX idx_kb_chunks_tsv ON kb.DocumentChunks USING GIN(content_tsv);
CREATE INDEX idx_kb_chunks_embedding ON kb.DocumentChunks USING hnsw (embedding vector_cosine_ops);
```

### Step 4.2: Run schema on startup

In `Program.cs`:

```csharp
app.UseDatabaseMigrations();
```

### Step 4.3: Add ingestion provenance and trust metadata

For `kb.Documents` and `kb.DocumentChunks`, include trust metadata to keep retrieval safe and auditable:

- `source_owner` (team/system that owns the source)
- `source_url` (canonical path or repo link)
- `content_hash` (detect stale chunks)
- `classification` (`public-internal`, `internal`, `restricted`)
- `reviewed_at` and `reviewed_by` (freshness and accountability)

---

## Phase 5: Agent & Standards Content

### Step 5.1: Agent Definitions

Create `src/NafMcp.Mcp/agents/` folder with agent definition files.

Each agent is a markdown file with frontmatter:

```markdown
---
name: planning-agent
description: Break down requirements into actionable tasks
scope: planning
tags: [requirements, tasks, breakdown]
models: [claude-sonnet-4-7]
---
# Planning Agent

Your role is to break down requirements into the smallest actionable units.

Instructions:
- Ask clarifying questions first
- Break into tasks with clear acceptance criteria
- Identify dependencies between tasks
- Flag edge cases and assumptions

When to use:
- Starting any new feature or bug fix
- Planning a complex implementation
```

### Step 5.2: Standards Documents

Create `knowledge/` folder structure:

```
knowledge/
├── official/              # Industry standards
│   └── owasp-top-10.md
├── organization/          # Team standards
│   ├── csharp-standards.md
│   ├── typescript-standards.md
│   ├── code-review-checklist.md
│   └── sdlc-workflow.md
└── project/              # Project-specific (per repo)
    └── (empty, populated per repo)
```

### Step 5.3: Knowledgebase Index + Embeddings

Create `src/NafMcp.Mcp/Services/KnowledgebaseIndexer.cs`:

```csharp
public class KnowledgebaseIndexer
{
    private readonly IEmbeddingProvider _embeddings;
    private readonly NpgsqlConnection _db;

    public async Task IndexAsync(string path)
    {
        // Scan knowledge/ folder for canonical docs
        // Parse entities/relations/workflows into kb.Entities + kb.Relations
        // Chunk documents and store in kb.DocumentChunks
        // Generate embeddings for each chunk
    }

    public async Task<List<KbSearchResult>> SearchAsync(string query)
    {
        // Hybrid retrieval:
        // 1) Full-text match via tsvector
        // 2) Vector similarity match via pgvector
        // 3) Expand related entities and workflows
    }
}
```

---

## Phase 6: Security & Modernization Baseline

### Step 6.1: Secure MCP endpoint access

- Local development: allow localhost only.
- Shared/production: require authenticated access (API key, Entra ID, or gateway auth).
- Restrict CORS and transport to HTTPS only outside local dev.
- Add rate limiting per client to reduce abuse risk.

### Step 6.2: Secret management and configuration safety

- Keep all credentials in environment variables or secret store (never in source).
- For Azure deployment, store secrets in Key Vault and reference via managed identity.
- Add startup validation so missing required config fails fast with clear errors.

### Step 6.3: Harden ingestion and tool outputs

- Ingest only from approved source paths/repositories (allowlist).
- Validate and sanitize markdown/file inputs before indexing.
- Enforce max document and chunk size limits to avoid abuse.
- Return source citations (`source_path`, `section`, `updated_at`) in `kb_search` responses.

### Step 6.4: Modernize dependency and security lifecycle

Add `Directory.Packages.props` and centralize package versions:

```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting" Version="9.0.0" />
    <PackageVersion Include="Aspire.Hosting.Postgres" Version="9.0.0" />
    <PackageVersion Include="ModelContextProtocol" Version="1.2.0" />
    <PackageVersion Include="ModelContextProtocol.AspNetCore" Version="1.2.0" />
    <PackageVersion Include="Npgsql" Version="9.0.0" />
    <PackageVersion Include="Pgvector" Version="0.2.2" />
  </ItemGroup>
</Project>
```

Then enforce recurring checks:

- Weekly dependency update PR (Dependabot or Renovate)
- `dotnet list package --vulnerable --include-transitive`
- NuGet audit in CI as a required check
- Quarterly template refresh review (`dotnet new mcpserver` diff against host layer)

### Step 6.5: Add operational security checks

- Add structured audit logs for tool calls (without sensitive payload leakage).
- Add health/readiness checks for DB and embedding provider dependencies.
- Define incident fallback: disable `kb_refresh_index` and high-risk tools via feature flags.

---

## Phase 7: Connecting VS Code

### Step 6.1: MCP connection config

Create `.vscode/mcp.json` template:

```json
{
    "servers": {
        "naf-mcp": {
            "type": "sse",
            "url": "http://localhost:5000/mcp"
        }
    }
}
```

### Step 6.2: Developer setup instructions

```markdown
# NAF MCP Server - Developer Setup

## Quick Connect

1. Open VS Code
2. Press Ctrl+Shift+P
3. Type "MCP: Add Server"
4. Enter: `naf-mcp`
5. URL: `http://YOUR-SERVER:5000/mcp`

## Or manual config

Add to `.vscode/mcp.json`:

```json
{
    "servers": {
        "naf-mcp": {
            "type": "sse", 
            "url": "http://YOUR-SERVER:5000/mcp"
        }
    }
}
```

---

## Phase 8: Hosting Options

### Option A: Local (development)

```bash
aspire start
```

### Option B: Shared dev machine

- Host on a shared development machine
- All devs connect to `http://DEV-MACHINE:5000/mcp`

### Option C: Container (local Docker)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspire:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "NafMcp.Mcp.dll"]
```

```yaml
# docker-compose.yml
services:
  naf-mcp:
    build: .
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__postgres=Host=POSTGRES;Database=naf
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: naf
      POSTGRES_PASSWORD: CHANGE_ME
    volumes:
      - postgres-data:/var/lib/postgresql/data
```

---

## Phase 9: Azure Hosting (Production)

Aspire has built-in Azure resource types — no custom config needed.

### Step 8.1: Add Azure packages

In `NafMcp.AppHost.csproj`:

```xml
<PackageReference Include="Aspire.Hosting.Azure" Version="9.0.0" />
```

### Step 8.2: Configure AppHost for Azure

In `Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Use Azure Postgres instead of container Postgres
var postgres = builder.AddAzurePostgresFlexibleServer("naf-postgres")
    .AddDatabase("naf");

var mcp = builder.AddProject<NafMcp.Mcp.Project>("naf-mcp")
    .WithReference(postgres);

builder.Build().Run();
```

### Step 8.3: Azure deployment manifest

Generate manifest:

```bash
aspire do publish-manifest --output-path ./azure-manifest.json
```

Deploy with Azure Developer CLI:

```bash
azd up
```

Or manually to Azure Container Apps:

```bash
# Login
azd auth login

# Deploy
azd provision --environment naf
azd deploy
```

### Step 8.4: Azure resources provisioned

| Resource | Type | Notes |
|----------|------|-------|
| Azure Postgres Flexible Server | Database | Managed, scalable |
| Azure Container Apps | Compute | Serverless |
| (optional) Azure Key Vault | Secrets | Store connection strings |

### Step 8.5: Configure connection for devs

After Azure deployment, update `.vscode/mcp.json`:

```json
{
    "servers": {
        "naf-mcp": {
            "type": "sse",
            "url": "https://naf-mcp.azurecontainerapps.io/mcp"
        }
    }
}
```

---

## Phase 10: External MCP Gateway (Federation)

Use NAF MCP as a secure gateway that can orchestrate other MCP servers (Azure DevOps, Figma, SonarQube, etc.) without coupling core logic to vendor-specific APIs.

### Step 10.1: Configure external connectors

In `appsettings.json`, define connectors with environment-backed auth headers:

```json
{
  "McpOptions": {
    "ExternalConnectors": [
      {
        "Name": "azure-devops",
        "Enabled": true,
        "Transport": "http",
        "Endpoint": "http://localhost:8794/mcp",
        "TimeoutMs": 30000,
        "Headers": {
          "Authorization": "Bearer ${env:AZURE_DEVOPS_PAT}"
        }
      },
      {
        "Name": "figma",
        "Enabled": true,
        "Transport": "http",
        "Endpoint": "http://localhost:8795/mcp",
        "TimeoutMs": 30000,
        "Headers": {
          "X-Figma-Token": "${env:FIGMA_ACCESS_TOKEN}"
        }
      },
      {
        "Name": "sonarqube",
        "Enabled": true,
        "Transport": "http",
        "Endpoint": "http://localhost:8796/mcp",
        "TimeoutMs": 30000,
        "Headers": {
          "Authorization": "Bearer ${env:SONARQUBE_TOKEN}"
        }
      }
    ]
  }
}
```

### Step 10.2: Expose gateway tools for discovery and passthrough

Add connector tools in `src/NafMcp.Mcp/Tools/ConnectorTools.cs`:

- `list_external_connectors`
- `list_external_mcp_tools`
- `call_external_mcp_tool`

These are the generic bridge layer and should remain stable even as downstream connectors evolve.

### Step 10.3: Add curated domain tools on top of passthrough

Create focused wrappers that call downstream MCP tools and normalize responses:

- `ado_get_work_items`, `ado_get_build_status`
- `figma_get_file_summary`, `figma_list_components`
- `sonar_get_quality_gate`, `sonar_list_hotspots`

Design requirement:

- Return a common response shape (`source`, `summary`, `details`, `citations`, `retrievedAt`) so AI consumers are insulated from vendor payload changes.

### Step 10.4: Sync high-value external data into KB

Schedule ingest jobs to persist durable context from external systems:

- ADO: work item taxonomy, pipeline definitions, release status
- Figma: design system tokens/components metadata
- SonarQube: project quality gate trends and critical hotspots

Persist to `kb.Documents`/`kb.DocumentChunks` (for retrieval) and `kb.Entities`/`kb.Relations` (for product/workflow graph context).

### Step 10.5: Gateway security and reliability controls

- Least-privilege token scopes per connector.
- Connector allowlist per MCP tool (prevent arbitrary cross-system calls).
- Retry/backoff + circuit breaker for downstream outages.
- Per-connector timeout budgets and rate limits.
- Audit logs for connector/tool/action/outcome (without sensitive payload contents).

---

### Hosting Comparison

| Option | Complexity | Cost | Best for |
|--------|------------|------|----------|
| Local | Easiest | Free | Dev/testing |
| Shared dev machine | Medium | Low | Small team |
| Container (self-hosted) | Medium | Server cost | Self-controlled |
| Azure Container Apps | Higher | Pay-for-use | Production |

**Recommendation for NAF:**

1. Start local for development
2. Move to shared dev machine for team
3. Consider Azure when team grows

---

## Complete File Structure

```
NafMcp/
├── src/
│   ├── NafMcp.Mcp/
│   │   ├── Program.cs
│   │   ├── Tools/
│   │   │   ├── MemoryTools.cs
│   │   │   ├── AgentTools.cs
│   │   │   ├── DocumentTools.cs
│   │   │   ├── KnowledgeTools.cs
│   │   │   └── ConnectorTools.cs
│   │   ├── Services/
│   │   │   ├── KnowledgebaseIndexer.cs
│   │   │   ├── KnowledgebaseService.cs
│   │   │   ├── ExternalConnectorRegistry.cs
│   │   │   ├── ExternalMcpClientService.cs
│   │   │   ├── DocumentIndex.cs
│   │   │   └── AgentCatalog.cs
│   │   ├── Schema/
│   │   │   └── schema.sql
│   │   ├── NafMcp.Mcp.csproj
│   │   └── agents/                    # Agent definitions
│   │       ├── planning-agent.md
│   │       ├── implementation-agent.md
│   │       ├── review-agent.md
│   │       └── testing-agent.md
│   └── NafMcp.AppHost/
│       ├── Program.cs
│       └── NafMcp.AppHost.csproj
├── knowledge/                           # Standards documents
│   ├── organization/
│   │   ├── csharp.md
│   │   ├── typescript.md
│   │   ├── angular.md
│   │   ├── react.md
│   │   └── sdlc-workflow.md
│   └── official/
│       └── owasp-top-10.md
├── .vscode/
│   └── mcp.json.template
├── docker-compose.yml
├── NafMcp.sln
└── README.md
```

---

## Implementation Checklist

- [ ] Create .NET 9 solution structure
- [ ] Install Aspire workload and scaffold from template (or Aspire CLI)
- [ ] Configure AppHost with Postgres
- [ ] Implement MCP server endpoints
- [ ] Create memory tools (recall, persist, link, status)
- [ ] Create agent tools (list, get, recommend)
- [ ] Create document tools (search, read, list)
- [ ] Create knowledgebase tools (kb_search, kb_get_entity, kb_get_workflow, kb_refresh_index)
- [ ] Initialize Postgres schema
- [ ] Add trust metadata fields (owner/source/classification/hash/review) to KB records
- [ ] Add agent definitions
- [ ] Add standards documents
- [ ] Enable pgvector and build knowledge chunk embedding pipeline
- [ ] Enforce authenticated access for non-local MCP endpoint
- [ ] Store secrets in env/secret manager only (no secrets in repo)
- [ ] Add dependency vulnerability checks to CI
- [ ] Add weekly package update automation (Dependabot/Renovate)
- [ ] Add source citations to KB retrieval results
- [ ] Configure external MCP connectors (ADO, Figma, SonarQube) with env-based auth
- [ ] Add gateway passthrough tools (list connectors/tools, call external tool)
- [ ] Add curated connector wrappers with normalized response shape
- [ ] Add sync jobs to ingest high-value external connector data into KB
- [ ] Add gateway controls (allowlist, timeout/rate limits, audit logging, retries)
- [ ] Test locally with Aspire
- [ ] Configure hosting (local/shared/container)
- [ ] Document VS Code connection for devs
- [ ] Announce to team

---

## Notes

- Uses .NET Aspire for local orchestration
- Postgres for memory + canonical NAF tech knowledgebase
- pgvector for semantic retrieval across product/workflow context
- Federated external MCP connectors through secure gateway tools
- Secure-by-default access, secrets, and ingestion guardrails
- Ongoing modernization through dependency automation and security audits
- MCP protocol for tool exposure
- No external dependencies on this repo
- Starts simple, add more tools/agents as needed
