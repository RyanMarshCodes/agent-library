using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace Ryan.MCP.Mcp.McpTools;

[McpServerToolType]
public sealed partial class FetchTools(IHttpClientFactory httpClientFactory, ILogger<FetchTools> logger)
{
    private const int DefaultMaxLength = 50_000;
    private const int AbsoluteMaxLength = 200_000;

    private static readonly JsonSerializerOptions JsonOptions = new();

    [McpServerTool(Name = "fetch")]
    [Description(
        "Fetch a URL and return its content as clean text. " +
        "HTML pages are stripped of scripts, styles, and tags — only readable text is returned. " +
        "JSON and plain-text responses are returned as-is. Ideal for pulling documentation, " +
        "NuGet package pages, GitHub READMEs, or MS Learn articles into context.")]
    public async Task<string> Fetch(
        [Description("The URL to fetch (must be http or https)")] string url,
        [Description("Maximum characters to return (default 50000, max 200000)")] int? maxLength = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Err("url is required");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Err("url must be a valid http or https URL");

        var limit = Math.Clamp(maxLength ?? DefaultMaxLength, 1, AbsoluteMaxLength);
        var client = httpClientFactory.CreateClient("fetch");

        try
        {
            using var response = await client
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (!response.IsSuccessStatusCode)
            {
                return JsonSerializer.Serialize(new
                {
                    url,
                    statusCode = (int)response.StatusCode,
                    error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}",
                }, JsonOptions);
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            string content;
            string format;

            if (contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
            {
                content = ExtractTextFromHtml(raw);
                format = "html→text";
            }
            else
            {
                content = raw;
                format = contentType is { Length: > 0 } ? contentType : "text";
            }

            var truncated = content.Length > limit;

            return JsonSerializer.Serialize(new
            {
                url,
                statusCode = (int)response.StatusCode,
                contentType,
                format,
                totalLength = content.Length,
                truncated,
                truncatedAt = truncated ? (int?)limit : null,
                content = truncated ? content[..limit] : content,
            }, JsonOptions);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Err($"Request to {url} timed out");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Fetch failed for {Url}", url);
            return Err($"Network error: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error fetching {Url}", url);
            return Err($"Unexpected error: {ex.Message}");
        }
    }

    private static string Err(string message) =>
        JsonSerializer.Serialize(new { error = message });

    private static string ExtractTextFromHtml(string html)
    {
        // Remove <script> and <style> blocks entirely
        var text = ScriptPattern().Replace(html, string.Empty);
        text = StylePattern().Replace(text, string.Empty);
        text = CommentPattern().Replace(text, string.Empty);

        // Block-level elements → newlines so paragraphs separate naturally
        text = BlockClosePattern().Replace(text, "\n");
        text = LineBreakPattern().Replace(text, "\n");

        // Strip all remaining tags
        text = TagPattern().Replace(text, string.Empty);

        // Decode HTML entities (&amp; &lt; &nbsp; etc.)
        text = WebUtility.HtmlDecode(text);

        // Normalize whitespace
        text = InlineWhitespacePattern().Replace(text, " ");
        text = ExcessiveNewlinesPattern().Replace(text, "\n\n");

        return text.Trim();
    }

    // Source-generated regexes — compiled once, allocation-free matching
    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptPattern();

    [GeneratedRegex(@"<style[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex StylePattern();

    [GeneratedRegex(@"<!--[\s\S]*?-->")]
    private static partial Regex CommentPattern();

    [GeneratedRegex(@"</(p|div|h[1-6]|li|tr|td|th|blockquote|pre|section|article|header|footer|nav|main)\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockClosePattern();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakPattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"[ \t]+")]
    private static partial Regex InlineWhitespacePattern();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesPattern();
}
