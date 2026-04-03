using Npgsql;

namespace Ryan.MCP.Mcp.Services.Memory;

public sealed class MemoryMigrationRunner(
    NpgsqlDataSource dataSource,
    ILogger<MemoryMigrationRunner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var migrationsDir = Path.Combine(AppContext.BaseDirectory, "Database", "Migrations");
        if (!Directory.Exists(migrationsDir))
        {
            throw new DirectoryNotFoundException($"Migrations directory not found: {migrationsDir}");
        }

        var sqlFiles = Directory.GetFiles(migrationsDir, "*.sql")
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (sqlFiles.Count == 0)
        {
            logger.LogWarning("No migration files found in {Dir}", migrationsDir);
            return;
        }

        Exception? lastError = null;
        for (var attempt = 1; attempt <= 20; attempt++)
        {
            try
            {
                await using var conn = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

                foreach (var sqlPath in sqlFiles)
                {
                    var sql = await File.ReadAllTextAsync(sqlPath, cancellationToken).ConfigureAwait(false);
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogInformation("Migration applied: {File}", Path.GetFileName(sqlPath));
                }

                return;
            }
            catch (NpgsqlException ex)
            {
                lastError = ex;
                logger.LogWarning(ex, "Migration attempt {Attempt} failed (connection); retrying", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(1 + attempt, 5)), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("Unable to connect to postgres backend after retries.", lastError);
    }
}
