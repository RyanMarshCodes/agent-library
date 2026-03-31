using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Ryan.MCP.Mcp.Configuration;

namespace Ryan.MCP.Mcp.Services;

/// <summary>
/// Invokes tools on external MCP servers configured in <see cref="McpOptions.ExternalConnectors"/>.
/// </summary>
public sealed partial class ExternalMcpClientService(
    ExternalConnectorRegistry registry,
    ILoggerFactory loggerFactory,
    ILogger<ExternalMcpClientService> logger)
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new() { WriteIndented = true };

    [GeneratedRegex(@"\$\{env:([^}]+)\}", RegexOptions.CultureInvariant)]
    private static partial Regex EnvPlaceholderRegex();

    public async Task<string> ListToolsAsync(string connectorName, CancellationToken cancellationToken = default)
    {
        if (!registry.TryGetEnabled(connectorName, out var connector))
        {
            return JsonSerializer.Serialize(new
            {
                error = $"No enabled external connector named '{connectorName}'. Use list_external_connectors.",
            }, JsonWriteOptions);
        }

        if (!TryCreateEndpointUri(connector, out var endpointError, out var endpointUri))
        {
            return JsonSerializer.Serialize(new { error = endpointError }, JsonWriteOptions);
        }

        ArgumentNullException.ThrowIfNull(endpointUri);

        try
        {
            await using var client = await CreateClientAsync(connector, endpointUri, cancellationToken).ConfigureAwait(false);
            var tools = await client.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Serialize(new
            {
                connector = connectorName,
                count = tools.Count,
                tools = tools.Select(t => new { t.Name, t.Description }),
            }, JsonWriteOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ListTools failed for external connector {Connector}", connectorName);
            return JsonSerializer.Serialize(new
            {
                error = $"Failed to list tools on '{connectorName}': {ex.Message}",
            }, JsonWriteOptions);
        }
    }

    public async Task<string> CallToolAsync(
        string connectorName,
        string toolName,
        string? argumentsJson,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return JsonSerializer.Serialize(new { error = "toolName is required" }, JsonWriteOptions);
        }

        if (!registry.TryGetEnabled(connectorName, out var connector))
        {
            return JsonSerializer.Serialize(new
            {
                error = $"No enabled external connector named '{connectorName}'. Use list_external_connectors.",
            }, JsonWriteOptions);
        }

        if (!TryCreateEndpointUri(connector, out var endpointError, out var endpointUri))
        {
            return JsonSerializer.Serialize(new { error = endpointError }, JsonWriteOptions);
        }

        ArgumentNullException.ThrowIfNull(endpointUri);

        IReadOnlyDictionary<string, object?>? args = null;
        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            try
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson);
                if (raw is { Count: > 0 })
                {
                    args = raw.ToDictionary(
                        static kv => kv.Key,
                        static kv => (object?)kv.Value,
                        StringComparer.Ordinal);
                }
            }
            catch (JsonException jx)
            {
                return JsonSerializer.Serialize(new { error = $"Invalid argumentsJson: {jx.Message}" }, JsonWriteOptions);
            }
        }

        try
        {
            await using var client = await CreateClientAsync(connector, endpointUri, cancellationToken).ConfigureAwait(false);
            var result = await client.CallToolAsync(toolName, args, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return FormatCallToolResult(result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "CallTool failed for {Connector}/{Tool}", connectorName, toolName);
            return JsonSerializer.Serialize(new
            {
                error = $"Tool call failed on '{connectorName}' ({toolName}): {ex.Message}",
            }, JsonWriteOptions);
        }
    }

    private async Task<McpClient> CreateClientAsync(
        ExternalMcpConnectorOptions connector,
        Uri endpointUri,
        CancellationToken cancellationToken)
    {
        var transportOpts = new HttpClientTransportOptions
        {
            Endpoint = endpointUri,
            Name = $"ext:{connector.Name}",
            ConnectionTimeout = TimeSpan.FromMilliseconds(Math.Clamp(connector.TimeoutMs, 1000, 600_000)),
            AdditionalHeaders = ResolveHeaders(connector.Headers),
            TransportMode = HttpTransportMode.AutoDetect,
        };

        var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(Math.Clamp(connector.TimeoutMs, 1000, 600_000)) };
        var transport = new HttpClientTransport(transportOpts, httpClient, loggerFactory, ownsHttpClient: true);

        // Node MCP servers (e.g. @modelcontextprotocol/server-memory behind supergateway) often lag the C# SDK default.
        // Request a version they advertise or initialize fails with HTTP 400 (protocol version mismatch).
        var clientOptions = new McpClientOptions
        {
            ProtocolVersion = "2025-06-18",
        };

        return await McpClient.CreateAsync(transport, clientOptions, loggerFactory, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryCreateEndpointUri(
        ExternalMcpConnectorOptions connector,
        out string? error,
        out Uri? uri)
    {
        error = null;
        uri = null;
        if (string.IsNullOrWhiteSpace(connector.Endpoint))
        {
            error = $"Connector '{connector.Name}' has no Endpoint configured.";
            return false;
        }

        if (!Uri.TryCreate(connector.Endpoint, UriKind.Absolute, out var u)
            || (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps))
        {
            error = $"Connector '{connector.Name}' Endpoint must be an absolute http(s) URL.";
            return false;
        }

        uri = u;
        return true;
    }

    private static Dictionary<string, string> ResolveHeaders(Dictionary<string, string> headers)
    {
        if (headers.Count == 0)
        {
            return [];
        }

        var rx = EnvPlaceholderRegex();
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key] = rx.Replace(value, m =>
            {
                var name = m.Groups[1].Value.Trim();
                return Environment.GetEnvironmentVariable(name) ?? string.Empty;
            });
        }

        return result;
    }

    private static string FormatCallToolResult(CallToolResult result)
    {
        if (result.StructuredContent is JsonElement structured)
        {
            return structured.GetRawText();
        }

        var texts = result.Content.OfType<TextContentBlock>().Select(t => t.Text).ToList();
        if (texts.Count > 0)
        {
            return texts.Count == 1 ? texts[0] : string.Join("\n", texts);
        }

        return JsonSerializer.Serialize(new
        {
            result.IsError,
            content = result.Content,
        }, JsonWriteOptions);
    }
}
