using Npgsql;

namespace Ryan.MCP.Mcp.Services.Memory;

public sealed class MemoryMigrationRunner(
    NpgsqlDataSource dataSource,
    ILogger<MemoryMigrationRunner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", "Migrations", "001_init_memory.sql");
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("Memory migration script not found", sqlPath);
        }

        var sql = await File.ReadAllTextAsync(sqlPath, cancellationToken).ConfigureAwait(false);
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 20; attempt++)
        {
            try
            {
                await using var conn = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Memory migration applied from {Path}", sqlPath);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;
                logger.LogWarning(ex, "Memory migration attempt {Attempt} failed; retrying", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(1 + attempt, 5)), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("Unable to connect to postgres memory backend after retries.", lastError);
    }
}
