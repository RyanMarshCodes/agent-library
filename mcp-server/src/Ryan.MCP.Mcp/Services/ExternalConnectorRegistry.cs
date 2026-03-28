using Ryan.MCP.Mcp.Configuration;

namespace Ryan.MCP.Mcp.Services;

public sealed class ExternalConnectorRegistry
{
    private readonly List<ExternalMcpConnectorOptions> _configured;
    private readonly List<ExternalMcpConnectorOptions> _enabled;

    public ExternalConnectorRegistry(McpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var overrides = options.ExternalConnectorEndpointOverrides;

        _configured = options.ExternalConnectors
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => ApplyOverrides(x, overrides))
            .ToList();
        _enabled = _configured.Where(x => x.Enabled).ToList();
    }

    public IReadOnlyList<ExternalMcpConnectorOptions> Configured => _configured;

    public IReadOnlyList<ExternalMcpConnectorOptions> Enabled => _enabled;

    private static ExternalMcpConnectorOptions ApplyOverrides(
        ExternalMcpConnectorOptions source,
        IReadOnlyDictionary<string, string> endpointOverrides)
    {
        var clone = new ExternalMcpConnectorOptions
        {
            Name = source.Name,
            Enabled = source.Enabled,
            Transport = source.Transport,
            Endpoint = source.Endpoint,
            TimeoutMs = source.TimeoutMs,
            Headers = new Dictionary<string, string>(source.Headers, StringComparer.OrdinalIgnoreCase),
        };

        if (endpointOverrides.TryGetValue(source.Name, out var overrideEndpoint) && !string.IsNullOrWhiteSpace(overrideEndpoint))
        {
            clone.Endpoint = overrideEndpoint;
        }

        return clone;
    }
}
