using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Ryan.MCP.Mcp.Services;

namespace Ryan.MCP.Mcp.McpTools;

[McpServerToolType]
public sealed class DocumentTools(DocumentIngestionCoordinator documents)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_standards")]
    [Description("List all indexed standards and knowledge documents by tier (official, organization, project). Returns relative file paths. Use read_document to fetch content.")]
    public string ListStandards(
        [Description("Filter by tier: 'official', 'organization', or 'project'. Omit to list all.")] string? tier = null,
        [Description("Filter by language or subdirectory prefix (e.g. 'csharp', 'typescript'). Omit to list all.")] string? language = null)
    {
        var snapshot = documents.Snapshot;
        var docs = snapshot.Documents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(tier))
            docs = docs.Where(d => d.Tier.Equals(tier, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(language))
            docs = docs.Where(d => d.RelativePath.StartsWith(language + "/", StringComparison.OrdinalIgnoreCase)
                                || d.RelativePath.StartsWith(language + "\\", StringComparison.OrdinalIgnoreCase));

        var results = docs.ToList();
        var byTier = results
            .GroupBy(d => d.Tier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(d => d.RelativePath).OrderBy(p => p).ToList());

        return JsonSerializer.Serialize(new
        {
            snapshot.ProjectSlug,
            totalDocuments = results.Count,
            snapshot.ByTier,
            snapshot.Roots,
            filter = new { tier, language },
            documents = byTier,
        }, JsonOptions);
    }

    [McpServerTool(Name = "read_document")]
    [Description("Read the full content of a standards document by tier and relative path. Use list_standards or documents://list to discover available paths.")]
    public async Task<string> ReadDocument(
        [Description("Document tier: 'official', 'organization', or 'project'")] string tier,
        [Description("Relative path as returned by list_standards, e.g. 'csharp/async-programming.md'")] string path,
        CancellationToken cancellationToken)
    {
        var entry = documents.Snapshot.Documents.FirstOrDefault(d =>
            d.Tier.Equals(tier, StringComparison.OrdinalIgnoreCase) &&
            d.RelativePath.Equals(path.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            return JsonSerializer.Serialize(new
            {
                error = $"Document '{path}' not found in tier '{tier}'.",
                hint = "Use list_standards to see available documents and their exact paths.",
            });
        }

        try
        {
            var content = await File.ReadAllTextAsync(entry.AbsolutePath, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Serialize(new
            {
                tier = entry.Tier,
                path = entry.RelativePath,
                sizeBytes = entry.SizeBytes,
                lastWriteUtc = entry.LastWriteUtc,
                content,
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Failed to read document: {ex.Message}" });
        }
    }

    [McpServerTool(Name = "search_documents")]
    [Description("Search standards documents by keyword across file paths and content. Returns matching documents with the tier and path needed to read them.")]
    public async Task<string> SearchDocuments(
        [Description("Search query, e.g. 'async await' or 'dependency injection' or 'error handling'")] string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return JsonSerializer.Serialize(new { error = "query is required" });

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = new List<object>();

        foreach (var doc in documents.Snapshot.Documents)
        {
            var pathMatch = terms.Any(t => doc.RelativePath.Contains(t, StringComparison.OrdinalIgnoreCase));
            if (pathMatch)
            {
                results.Add(new { doc.Tier, doc.RelativePath, matchedOn = "path", readWith = $"read_document(\"{doc.Tier}\", \"{doc.RelativePath}\")" });
                continue;
            }

            try
            {
                var content = await File.ReadAllTextAsync(doc.AbsolutePath, cancellationToken).ConfigureAwait(false);
                if (terms.Any(t => content.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    results.Add(new { doc.Tier, doc.RelativePath, matchedOn = "content", readWith = $"read_document(\"{doc.Tier}\", \"{doc.RelativePath}\")" });
            }
            catch
            {
                // Skip unreadable files silently
            }
        }

        return JsonSerializer.Serialize(new { query, count = results.Count, results }, JsonOptions);
    }

    [McpServerTool(Name = "ingestion_status")]
    [Description("Get the current document ingestion status — when documents were last indexed and how many are indexed per tier.")]
    public string IngestionStatus()
    {
        var snapshot = documents.Snapshot;
        return JsonSerializer.Serialize(new
        {
            snapshot.ProjectSlug,
            snapshot.LastStartedUtc,
            snapshot.UpdatedUtc,
            snapshot.TotalDocuments,
            snapshot.ByTier,
            snapshot.Roots,
        }, JsonOptions);
    }

    [McpServerTool(Name = "ingest_documents")]
    [Description("Trigger a re-scan and re-index of all knowledge documents. Use after adding or modifying documents.")]
    public async Task<string> IngestDocuments(CancellationToken cancellationToken)
    {
        await documents.TriggerReindexAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = documents.Snapshot;
        return JsonSerializer.Serialize(new
        {
            status = "ok",
            snapshot.ProjectSlug,
            snapshot.UpdatedUtc,
            snapshot.TotalDocuments,
            snapshot.ByTier,
        }, JsonOptions);
    }
}
