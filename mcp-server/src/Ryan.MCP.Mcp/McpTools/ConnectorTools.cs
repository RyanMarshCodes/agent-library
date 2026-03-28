using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Ryan.MCP.Mcp.Services;

namespace Ryan.MCP.Mcp.McpTools;

[McpServerToolType]
public sealed class ConnectorTools(ExternalConnectorRegistry connectors)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "list_external_connectors")]
    [Description("List all configured external MCP connectors (e.g. Azure DevOps, GitHub, Docker) and whether they are enabled.")]
    public string ListExternalConnectors()
    {
        return JsonSerializer.Serialize(new
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
        }, JsonOptions);
    }
}
