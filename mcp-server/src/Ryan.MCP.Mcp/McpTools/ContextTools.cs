using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;
using Ryan.MCP.Mcp.Services;

namespace Ryan.MCP.Mcp.McpTools;

[McpServerToolType]
public sealed class ContextTools(AgentIngestionCoordinator agents, DocumentIngestionCoordinator documents)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_context")]
    [Description("""
        The primary entry point for polyglot development. Given a language or framework and optional task,
        returns all relevant agents, standards, and recommendations in one call.
        Use this first when starting any task — it surfaces what's available for your stack and tells you exactly how to activate it.
        Supports any language: csharp, typescript, javascript, python, go, rust, java, kotlin, swift, react, angular, vue, etc.
        """)]
    public string GetContext(
        [Description("Programming language or framework (e.g. 'csharp', 'typescript', 'python', 'go', 'rust', 'java', 'kotlin', 'react'). Omit for universal context.")] string? language = null,
        [Description("Optional task description for targeted recommendations (e.g. 'security audit', 'refactoring', 'write tests', 'code review')")] string? task = null)
    {
        var agentSnapshot = agents.Snapshot;
        var docSnapshot = documents.Snapshot;

        // Collect relevant agents: task-based + language-based search
        var agentHits = new List<AgentEntry>();
        if (!string.IsNullOrWhiteSpace(task))
            agentHits.AddRange(agents.SearchAgents(task));
        if (!string.IsNullOrWhiteSpace(language))
        {
            foreach (var a in agents.SearchAgents(language))
                if (!agentHits.Any(x => x.Name == a.Name))
                    agentHits.Add(a);
        }

        // Fall back to all agents if no specific match
        var relevantAgents = agentHits.Count > 0
            ? agentHits
            : agentSnapshot.Agents.OrderBy(a => a.Name).ToList();

        // Collect relevant documents: language-path prefix + official (global) tier
        var relevantDocs = docSnapshot.Documents
            .Where(d =>
                d.Tier.Equals("official", StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(language) && (
                    d.RelativePath.StartsWith(language + "/", StringComparison.OrdinalIgnoreCase) ||
                    d.RelativePath.StartsWith(language + "\\", StringComparison.OrdinalIgnoreCase))))
            .OrderBy(d => d.Tier, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Also include task-relevant docs via keyword match on path
        if (!string.IsNullOrWhiteSpace(task))
        {
            var taskTerms = task.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var doc in docSnapshot.Documents)
            {
                if (relevantDocs.Any(d => d.AbsolutePath == doc.AbsolutePath))
                    continue;
                if (taskTerms.Any(t => doc.RelativePath.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    relevantDocs.Add(doc);
            }
        }

        return JsonSerializer.Serialize(new
        {
            language = language ?? "universal",
            task,
            agents = new
            {
                count = relevantAgents.Count,
                items = relevantAgents.Take(8).Select(a => new
                {
                    a.Name,
                    a.Description,
                    a.Scope,
                    a.Tags,
                    activate = $"use_agent(\"{a.Name}\")",
                }),
                seeAll = "list_agents()",
            },
            standards = new
            {
                count = relevantDocs.Count,
                items = relevantDocs.Select(d => new
                {
                    d.Tier,
                    d.RelativePath,
                    read = $"read_document(\"{d.Tier}\", \"{d.RelativePath}\")",
                    load = $"use_standard(\"{d.Tier}\", \"{d.RelativePath}\")",
                }),
                seeAll = "list_standards()",
            },
            quickStart = BuildQuickStart(language, task, relevantAgents, relevantDocs),
        }, JsonOptions);
    }

    private static string BuildQuickStart(
        string? language,
        string? task,
        IReadOnlyList<AgentEntry> relevantAgents,
        IReadOnlyList<IngestionEntry> relevantDocs)
    {
        var sb = new StringBuilder();
        sb.Append("# Quick Start");
        if (language != null) sb.Append($" — {language}");
        if (task != null) sb.Append($" — {task}");
        sb.AppendLine();
        sb.AppendLine();

        if (relevantAgents.Count > 0)
        {
            var top = relevantAgents[0];
            sb.AppendLine($"**Step 1 — Activate your agent**: `use_agent(\"{top.Name}\")`");
            sb.AppendLine($"> {top.Description}");
            sb.AppendLine();
        }

        if (relevantDocs.Count > 0)
        {
            sb.AppendLine("**Step 2 — Load relevant standards**:");
            foreach (var doc in relevantDocs.Take(5))
                sb.AppendLine($"- `read_document(\"{doc.Tier}\", \"{doc.RelativePath}\")`");
            sb.AppendLine();
        }

        if (relevantAgents.Count > 1)
        {
            sb.AppendLine("**Other available agents**:");
            foreach (var a in relevantAgents.Skip(1).Take(4))
                sb.AppendLine($"- `{a.Name}` — {a.Description}");
        }

        return sb.ToString();
    }
}
