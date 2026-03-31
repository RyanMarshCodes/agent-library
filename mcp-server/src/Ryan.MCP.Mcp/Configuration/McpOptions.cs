namespace Ryan.MCP.Mcp.Configuration;

public class McpOptions
{
    public const string SectionName = "McpOptions";

    public KnowledgeOptions Knowledge { get; set; } = new();
    public IngestionOptions Ingestion { get; set; } = new();
    public AgentOptions? Agents { get; set; }
    public MemoryStoreOptions MemoryStore { get; set; } = new();
    public List<ExternalMcpConnectorOptions> ExternalConnectors { get; set; } = [];
    public Dictionary<string, string> ExternalConnectorEndpointOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class MemoryStoreOptions
{
    public string Provider { get; set; } = "postgres";
    public string ConnectionString { get; set; } = "Host=localhost;Port=8810;Database=ryan_memory;Username=postgres;Password=postgres";
    public int CommandTimeoutSeconds { get; set; } = 30;
}

public class KnowledgeOptions
{
    public string ProjectSlug { get; set; } = "ryan-mcp";

    public string? OfficialPath { get; set; }

    public string? OrganizationPath { get; set; }

    public string? ProjectPath { get; set; }

    public string? IndexPath { get; set; }
}

public class IngestionOptions
{
    public bool AutoIngestOnStartup { get; set; } = true;
    public bool WatchForChanges { get; set; } = true;
    public int DebounceMilliseconds { get; set; } = 1500;
    public List<string> AllowedExtensions { get; set; } = [".md", ".txt", ".json", ".yaml", ".yml"];
}

public class AgentOptions
{
    public string? LocalPath { get; set; }

    public string? LibraryPath { get; set; }

    public string? ProjectPath { get; set; }
}

public class ExternalMcpConnectorOptions
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Transport { get; set; } = "http";
    public string Endpoint { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 8000;
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
