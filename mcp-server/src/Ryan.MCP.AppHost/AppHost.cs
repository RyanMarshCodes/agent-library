var builder = DistributedApplication.CreateBuilder(args);

// Resolve paths relative to repo root (3 levels up from AppHost dir)
var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
var agentsPath = Path.Combine(repoRoot, "ai-docs", "agents");
var globalDocsPath = Path.Combine(repoRoot, "global");
var backendDocsPath = Path.Combine(repoRoot, "backend");
var frontendDocsPath = Path.Combine(repoRoot, "frontend");
var localAgentsPath = Path.Combine(repoRoot, ".local", "agents");
var indexPath = Path.Combine(repoRoot, ".local", "mcp-index");

var mcpServer = builder.AddProject<Projects.Ryan_MCP_Mcp>("mcp-server")
    .WithEnvironment("McpOptions__Knowledge__ProjectSlug", "ryan-mcp")
    .WithEnvironment("McpOptions__Knowledge__OfficialPath", globalDocsPath)
    .WithEnvironment("McpOptions__Knowledge__OrganizationPath", backendDocsPath)
    .WithEnvironment("McpOptions__Knowledge__ProjectPath", frontendDocsPath)
    .WithEnvironment("McpOptions__Knowledge__IndexPath", indexPath)
    .WithEnvironment("McpOptions__Agents__LocalPath", localAgentsPath)
    .WithEnvironment("McpOptions__Agents__LibraryPath", agentsPath)
    .WithEnvironment("McpOptions__Ingestion__WatchForChanges", "true")
    .WithEnvironment("McpOptions__Ingestion__AutoIngestOnStartup", "true");

// ── Remote HTTP connectors (no containers — zero machine overhead) ────────────
//
// GitHub MCP: https://api.githubcopilot.com/mcp/
//   Set GITHUB_TOKEN env var and enable "github" in McpOptions.ExternalConnectors.
//   Requires GitHub Copilot subscription (individual, business, or enterprise).
//
// Azure DevOps MCP: https://github.com/microsoft/azure-devops-mcp
//   Run once: docker run -e AZURE_DEVOPS_PAT=... <image> --transport http --port 8794
//   Or host it elsewhere and point Endpoint at your URL.
//   Set AZURE_DEVOPS_PAT env var and enable "azure-devops" in ExternalConnectors.

// ── Docker containers (all disabled by default — opt in as needed) ────────────

// Sequential Thinking — chain-of-thought reasoning (enabled by default, very small)
var sequentialThinkingEnabled = builder.Configuration["Projects:SequentialThinking:Enabled"]?.ToLower() != "false";
if (sequentialThinkingEnabled)
{
    var sequentialThinking = builder.AddContainer("sequentialthinking", "mcp/sequentialthinking:latest")
        .WithEndpoint(name: "mcp", port: 8788, targetPort: 8788);

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__sequential-thinking",
        sequentialThinking.GetEndpoint("mcp"));
}

// Azure MCP — Azure resource management via Azure CLI credentials.
// Run `az login` on the host before starting. Covers subscriptions, resources,
// Key Vault, Storage, App Service, AKS, and more.
var azureEnabled = builder.Configuration["Projects:Azure:Enabled"]?.ToLower() == "true";
if (azureEnabled)
{
    var azureMcp = builder.AddContainer("azure-mcp", "mcr.microsoft.com/azure-mcp-server:latest")
        .WithEndpoint(name: "mcp", port: 8790, targetPort: 8790)
        .WithEnvironment("AZURE_AUTH_MODE", "cli");

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__azure",
        azureMcp.GetEndpoint("mcp"));
}

// Docker MCP — manage containers, images, volumes from within AI context.
var dockerEnabled = builder.Configuration["Projects:Docker:Enabled"]?.ToLower() == "true";
if (dockerEnabled)
{
    var dockerMcp = builder.AddContainer("docker-mcp", "mcp/server-docker:latest")
        .WithEndpoint(name: "mcp", port: 8791, targetPort: 8791)
        .WithBindMount(source: "/var/run/docker.sock", target: "/var/run/docker.sock", isReadOnly: true);

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__docker",
        dockerMcp.GetEndpoint("mcp"));
}

// Discord MCP — send notifications to Discord channels.
// Set DISCORD_BOT_TOKEN env var.
var discordEnabled = builder.Configuration["Projects:Discord:Enabled"]?.ToLower() == "true";
if (discordEnabled)
{
    var discordMcp = builder.AddContainer("discord-mcp", "ghcr.io/saseq/discord-mcp:latest")
        .WithEndpoint(name: "mcp", port: 8792, targetPort: 8792)
        .WithEnvironment("DISCORD_BOT_TOKEN", Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ?? "");

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__discord",
        discordMcp.GetEndpoint("mcp"));
}

// Filesystem MCP — controlled read/write access to files outside the project.
// Mounts the repo root as /data (read-only).
var filesystemEnabled = builder.Configuration["Projects:Filesystem:Enabled"]?.ToLower() == "true";
if (filesystemEnabled)
{
    var filesystemMcp = builder.AddContainer("filesystem-mcp", "mcp/server-filesystem:latest")
        .WithEndpoint(name: "mcp", port: 8793, targetPort: 8793)
        .WithBindMount(source: repoRoot, target: "/data", isReadOnly: true);

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__filesystem",
        filesystemMcp.GetEndpoint("mcp"));
}

// Fetch MCP — fetches any URL and returns clean text/markdown. Ideal for pulling
// NuGet docs, Angular docs, MS Learn articles, GitHub READMEs into AI context.
// No auth or configuration needed. Tiny stateless container.
var fetchEnabled = builder.Configuration["Projects:Fetch:Enabled"]?.ToLower() == "true";
if (fetchEnabled)
{
    var fetchMcp = builder.AddContainer("fetch-mcp", "mcp/fetch:latest")
        .WithHttpEndpoint(name: "mcp", port: 8800, targetPort: 8000)
        .WithEnvironment("HOST", "0.0.0.0")
        .WithEnvironment("PORT", "8000");

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__fetch",
        fetchMcp.GetEndpoint("mcp"));
}

// Memory MCP — persistent knowledge graph stored across sessions. Use it to
// remember architecture decisions, team conventions, tech debt notes, etc.
// No auth needed. Data persists in a Docker volume.
var memoryEnabled = builder.Configuration["Projects:Memory:Enabled"]?.ToLower() == "true";
if (memoryEnabled)
{
    var memoryMcp = builder.AddContainer("memory-mcp", "mcp/memory:latest")
        .WithHttpEndpoint(name: "mcp", port: 8801, targetPort: 8000)
        .WithEnvironment("HOST", "0.0.0.0")
        .WithEnvironment("PORT", "8000");

    mcpServer.WithEnvironment("McpOptions__ExternalConnectorEndpointOverrides__memory",
        memoryMcp.GetEndpoint("mcp"));
}

// Ollama — local LLM inference (very heavy, GPU recommended).
var ollamaEnabled = builder.Configuration["Projects:Ollama:Enabled"]?.ToLower() == "true";
if (ollamaEnabled)
{
    builder.AddContainer("ollama", "ollama/ollama:latest")
        .WithHttpEndpoint(port: 11434, targetPort: 11434)
        .WithEnvironment("OLLAMA_HOST", "0.0.0.0:11434");
}

// Qdrant — vector database for semantic search (heavy, enable only if needed).
var qdrantEnabled = builder.Configuration["Projects:Qdrant:Enabled"]?.ToLower() == "true";
if (qdrantEnabled)
{
    builder.AddContainer("qdrant", "qdrant/qdrant:latest")
        .WithHttpEndpoint(port: 6333, targetPort: 6333);
}

builder.Build().Run();
