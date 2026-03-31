using System.Reflection;
using Microsoft.Extensions.Options;
using Ryan.MCP.Mcp.Configuration;
using Ryan.MCP.Mcp.Services;

var builder = WebApplication.CreateBuilder(args);

// MCP Options
builder.Services.Configure<McpOptions>(builder.Configuration.GetSection(McpOptions.SectionName));
var mcpOptions = builder.Configuration.GetSection(McpOptions.SectionName).Get<McpOptions>() ?? new McpOptions();
builder.Services.AddSingleton(mcpOptions);

// Use PascalCase JSON for all Results.Ok() responses — the management UI accesses properties by PascalCase name
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = null);

// HTTP client for fetch tool
builder.Services.AddHttpClient("fetch", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Ryan.MCP/1.0 (MCP fetch tool; +https://github.com/RyanMarshCodes/agent-library)");
});

// Core services
builder.Services.AddSingleton<ExternalConnectorRegistry>();
builder.Services.AddSingleton<ExternalMcpClientService>();
builder.Services.AddSingleton<DocumentIngestionCoordinator>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DocumentIngestionCoordinator>());
builder.Services.AddSingleton<AgentIngestionCoordinator>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AgentIngestionCoordinator>());
builder.Services.AddSingleton<FileManagementService>();

// MCP Protocol (tools, resources, prompts discovered via attributes)
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(Assembly.GetExecutingAssembly())
    .WithPromptsFromAssembly(Assembly.GetExecutingAssembly())
    .WithResourcesFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseStaticFiles();

var connectorRegistry = app.Services.GetRequiredService<ExternalConnectorRegistry>();
app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation(
        "Ryan.MCP startup: {ConfiguredCount} connector(s) configured, {EnabledCount} enabled.",
        connectorRegistry.Configured.Count,
        connectorRegistry.Enabled.Count);
});

// ─── MCP Protocol endpoint (for IDEs / Claude Code / Cursor / OpenCode) ──────
app.MapMcp("/mcp");

// ─── Health ───────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Ryan.MCP.Mcp",
    utc = DateTime.UtcNow,
}));

// ─── REST API for management UI ───────────────────────────────────────────────

// Agents
app.MapGet("/api/agents", (AgentIngestionCoordinator agents, string? scope = null, string? tags = null) =>
{
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? []
        : tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

    var results = agents.ListAgents(scope, tagList);
    return Results.Ok(new
    {
        count = results.Count,
        scope,
        agents = results.Select(a => new
        {
            a.Name,
            a.Description,
            a.Scope,
            a.Tags,
            a.Format,
            a.FileName,
            a.SizeBytes,
            a.LastWriteUtc,
            a.IndexedUtc,
        }),
    });
});

app.MapGet("/api/agents/{name}", (string name, AgentIngestionCoordinator agents) =>
{
    var agent = agents.GetAgent(name);
    return agent == null
        ? Results.NotFound(new { error = $"Agent '{name}' not found" })
        : Results.Ok(agent);
});

app.MapGet("/api/agents/search", (string query, AgentIngestionCoordinator agents) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new { error = "query is required" });
    }

    var results = agents.SearchAgents(query);
    return Results.Ok(new { query, count = results.Count, agents = results });
});

app.MapGet("/api/agents/status", (AgentIngestionCoordinator agents) =>
{
    var s = agents.Snapshot;
    return Results.Ok(new
    {
        s.ProjectSlug,
        s.LastStartedUtc,
        s.UpdatedUtc,
        s.TotalAgents,
        s.ByScope,
        s.ByFormat,
        s.ScanRoots,
    });
});

app.MapPost("/api/agents/ingest", async (AgentIngestionCoordinator agents, CancellationToken ct) =>
{
    await agents.TriggerReindexAsync(ct).ConfigureAwait(false);
    var s = agents.Snapshot;
    return Results.Ok(new { status = "ok", s.UpdatedUtc, s.TotalAgents, s.ByScope });
});

app.MapPost("/api/agents/upload", async (HttpRequest request, FileManagementService files, AgentIngestionCoordinator agents, CancellationToken ct) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Multipart form required" });
    }

    var form = await request.ReadFormAsync(ct).ConfigureAwait(false);
    var uploaded = new List<object>();
    var failed = new List<object>();

    foreach (var file in form.Files)
    {
        await using var stream = file.OpenReadStream();
        var (success, message, filePath) = await files.SaveAgentAsync(file.FileName, stream, ct).ConfigureAwait(false);
        if (success)
        {
            uploaded.Add(new { file.FileName, message });
        }
        else
        {
            failed.Add(new { file.FileName, message });
        }
    }

    if (uploaded.Count > 0)
    {
        await agents.TriggerReindexAsync(ct).ConfigureAwait(false);
    }

    return Results.Ok(new { uploaded, failed });
});

app.MapDelete("/api/agents/{fileName}", async (string fileName, FileManagementService files, AgentIngestionCoordinator agents, CancellationToken ct) =>
{
    var (success, message) = files.DeleteAgent(fileName);
    if (!success)
    {
        return Results.NotFound(new { error = message });
    }

    await agents.TriggerReindexAsync(ct).ConfigureAwait(false);
    return Results.Ok(new { message });
});

// Documents
app.MapGet("/api/documents", (DocumentIngestionCoordinator docs) =>
{
    var s = docs.Snapshot;
    return Results.Ok(new
    {
        s.ProjectSlug,
        s.TotalDocuments,
        s.ByTier,
        s.Roots,
        s.LastStartedUtc,
        s.UpdatedUtc,
        documents = s.Documents.Select(d => new
        {
            d.Tier,
            d.RelativePath,
            d.SizeBytes,
            d.LastWriteUtc,
        }),
    });
});

app.MapPost("/api/documents/ingest", async (DocumentIngestionCoordinator docs, CancellationToken ct) =>
{
    await docs.TriggerReindexAsync(ct).ConfigureAwait(false);
    var s = docs.Snapshot;
    return Results.Ok(new { status = "ok", s.UpdatedUtc, s.TotalDocuments, s.ByTier });
});

app.MapPost("/api/documents/{tier}/upload", async (string tier, HttpRequest request, FileManagementService files, DocumentIngestionCoordinator docs, CancellationToken ct) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Multipart form required" });
    }

    var form = await request.ReadFormAsync(ct).ConfigureAwait(false);
    var uploaded = new List<object>();
    var failed = new List<object>();

    foreach (var file in form.Files)
    {
        await using var stream = file.OpenReadStream();
        var (success, message, filePath) = await files.SaveDocumentAsync(tier, file.FileName, stream, ct).ConfigureAwait(false);
        if (success)
        {
            uploaded.Add(new { file.FileName, message });
        }
        else
        {
            failed.Add(new { file.FileName, message });
        }
    }

    if (uploaded.Count > 0)
    {
        await docs.TriggerReindexAsync(ct).ConfigureAwait(false);
    }

    return Results.Ok(new { tier, uploaded, failed });
});

app.MapDelete("/api/documents/{tier}/{*relativePath}", async (string tier, string relativePath, FileManagementService files, DocumentIngestionCoordinator docs, CancellationToken ct) =>
{
    var (success, message) = files.DeleteDocument(tier, relativePath);
    if (!success)
    {
        return Results.NotFound(new { error = message });
    }

    await docs.TriggerReindexAsync(ct).ConfigureAwait(false);
    return Results.Ok(new { message });
});

// External connectors
app.MapGet("/api/connectors", (ExternalConnectorRegistry connectors) =>
    Results.Ok(new
    {
        configuredCount = connectors.Configured.Count,
        enabledCount = connectors.Enabled.Count,
        connectors = connectors.Configured.Select(c => new
        {
            c.Name,
            c.Enabled,
            c.Transport,
            c.Endpoint,
            c.TimeoutMs,
        }),
    }));

// Serve SPA for all other routes (management UI)
app.MapFallbackToFile("index.html");

app.Run();
