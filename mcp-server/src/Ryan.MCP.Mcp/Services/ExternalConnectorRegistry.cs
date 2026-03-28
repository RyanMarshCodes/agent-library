using Microsoft.Extensions.Options;
using Ryan.MCP.Mcp.Configuration;

namespace Ryan.MCP.Mcp.Services;

/// <summary>
/// Holds external MCP connector definitions from <see cref="McpOptions"/>.
/// Subscribes to configuration reloads so edits to appsettings (etc.) apply without restart.
/// </summary>
public sealed class ExternalConnectorRegistry : IDisposable
{
    private readonly IOptionsMonitor<McpOptions> _monitor;
    private readonly IDisposable? _onChange;
    private readonly object _gate = new();
    private IReadOnlyList<ExternalMcpConnectorOptions> _configured = [];
    private IReadOnlyList<ExternalMcpConnectorOptions> _enabled = [];

    public ExternalConnectorRegistry(IOptionsMonitor<McpOptions> monitor)
    {
        _monitor = monitor;
        Refresh(monitor.CurrentValue);
        _onChange = monitor.OnChange((opts, _) => Refresh(opts));
    }

    public IReadOnlyList<ExternalMcpConnectorOptions> Configured
    {
        get
        {
            lock (_gate)
            {
                return _configured;
            }
        }
    }

    public IReadOnlyList<ExternalMcpConnectorOptions> Enabled
    {
        get
        {
            lock (_gate)
            {
                return _enabled;
            }
        }
    }

    public void Dispose() => _onChange?.Dispose();

    private void Refresh(McpOptions? options)
    {
        options ??= _monitor.CurrentValue;
        ArgumentNullException.ThrowIfNull(options);

        var overrides = options.ExternalConnectorEndpointOverrides;

        var configured = options.ExternalConnectors
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => ApplyOverrides(x, overrides))
            .ToList();

        var enabled = configured.Where(x => x.Enabled).ToList();

        lock (_gate)
        {
            _configured = configured;
            _enabled = enabled;
        }
    }

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
