using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Ryan.MCP.Mcp.Services;

namespace Ryan.MCP.Mcp.McpTools;

[McpServerToolType]
public sealed class AgentTools(AgentIngestionCoordinator agents)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_agents")]
    [Description("List all available agents, skills, and instructions indexed by this MCP server. Returns name, description, scope, tags, and format for each.")]
    public string ListAgents(
        [Description("Filter by scope (e.g. 'refactoring', 'security', 'testing'). Omit to list all.")] string? scope = null,
        [Description("Comma-separated tags to filter by (e.g. 'csharp,dotnet'). Omit to list all.")] string? tags = null)
    {
        var tagList = string.IsNullOrWhiteSpace(tags)
            ? new List<string>()
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

        var results = agents.ListAgents(scope, tagList);
        return JsonSerializer.Serialize(new
        {
            count = results.Count,
            scope,
            tags = tagList,
            agents = results.Select(a => new
            {
                a.Name,
                a.Description,
                a.Scope,
                a.Tags,
                a.Format,
                a.FileName,
            }),
        }, JsonOptions);
    }

    [McpServerTool(Name = "get_agent")]
    [Description("Get the full content of a specific agent/skill/instruction by name. Returns the complete markdown including frontmatter and body.")]
    public string GetAgent([Description("The agent name exactly as returned by list_agents")] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return JsonSerializer.Serialize(new { error = "name is required" });
        }

        var agent = agents.GetAgent(name);
        if (agent == null)
        {
            return JsonSerializer.Serialize(new { error = $"Agent '{name}' not found. Use list_agents to see available agents." });
        }

        return JsonSerializer.Serialize(new
        {
            agent.Name,
            agent.Description,
            agent.Scope,
            agent.Tags,
            agent.Format,
            agent.FileName,
            agent.RawContent,
            agent.Frontmatter,
            agent.IndexedUtc,
        }, JsonOptions);
    }

    [McpServerTool(Name = "search_agents")]
    [Description("Search agents by keyword across name, description, tags, and file name. Use this to find the right agent for a task.")]
    public string SearchAgents([Description("Search query, e.g. 'security audit' or 'csharp refactor'")] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return JsonSerializer.Serialize(new { error = "query is required" });
        }

        var results = agents.SearchAgents(query);
        return JsonSerializer.Serialize(new
        {
            query,
            count = results.Count,
            agents = results.Select(a => new
            {
                a.Name,
                a.Description,
                a.Scope,
                a.Tags,
                a.Format,
                a.FileName,
            }),
        }, JsonOptions);
    }

    [McpServerTool(Name = "list_agent_scopes")]
    [Description("List all available agent scopes and formats, with counts. Use this to understand what categories of agents are available.")]
    public string ListAgentScopes()
    {
        var snapshot = agents.Snapshot;
        return JsonSerializer.Serialize(new
        {
            totalAgents = snapshot.TotalAgents,
            scopes = snapshot.ByScope.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value),
            formats = snapshot.ByFormat.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value),
            scanRoots = snapshot.ScanRoots,
        }, JsonOptions);
    }

    [McpServerTool(Name = "agent_status")]
    [Description("Get the current agent ingestion status including when agents were last indexed and how many are available.")]
    public string AgentStatus()
    {
        var snapshot = agents.Snapshot;
        return JsonSerializer.Serialize(new
        {
            snapshot.ProjectSlug,
            snapshot.LastStartedUtc,
            snapshot.UpdatedUtc,
            snapshot.TotalAgents,
            snapshot.ByScope,
            snapshot.ByFormat,
            snapshot.ScanRoots,
        }, JsonOptions);
    }

    [McpServerTool(Name = "recommend_agent")]
    [Description("Get the best agent recommendation for a specific task. Explains why the agent is recommended and how to activate it as your active persona.")]
    public string RecommendAgent([Description("Task description, e.g. 'security audit of my C# API' or 'refactor this class' or 'write unit tests'")] string task)
    {
        if (string.IsNullOrWhiteSpace(task))
            return JsonSerializer.Serialize(new { error = "task description is required" });

        var results = agents.SearchAgents(task);
        if (results.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                message = "No specific agent found for this task.",
                hint = "Use list_agents to browse all available agents, or search_agents with different keywords.",
            });
        }

        var top = results.First();
        return JsonSerializer.Serialize(new
        {
            task,
            recommendation = new
            {
                top.Name,
                top.Description,
                top.Scope,
                top.Tags,
                howToActivate = new
                {
                    asPrompt = $"use_agent(\"{top.Name}\")",
                    asSystemPrompt = $"use_agent_as_system(\"{top.Name}\")",
                    readFirst = $"get_agent(\"{top.Name}\")",
                    asResource = $"agents://{top.Name}",
                },
            },
            alternatives = results.Skip(1).Take(3).Select(a => new
            {
                a.Name,
                a.Description,
                activate = $"use_agent(\"{a.Name}\")",
            }),
        }, JsonOptions);
    }

    [McpServerTool(Name = "ingest_agents")]
    [Description("Trigger a re-scan and re-index of all agent files. Use this after adding or modifying agent files.")]
    public async Task<string> IngestAgents(CancellationToken cancellationToken)
    {
        await agents.TriggerReindexAsync(cancellationToken).ConfigureAwait(false);
        var snapshot = agents.Snapshot;
        return JsonSerializer.Serialize(new
        {
            status = "ok",
            snapshot.ProjectSlug,
            snapshot.UpdatedUtc,
            snapshot.TotalAgents,
            snapshot.ByScope,
            snapshot.ByFormat,
        }, JsonOptions);
    }
}
