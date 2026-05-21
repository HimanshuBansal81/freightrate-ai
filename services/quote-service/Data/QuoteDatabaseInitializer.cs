using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace quote_service.Data;

public static class QuoteDatabaseInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("QuoteDatabaseStartup");
        var dbContext = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
        var connectionString = dbContext.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Quote database connection string is required.");

        await EnsureDatabaseExistsAsync(connectionString, logger);

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                await QuoteDbSeeder.SeedAsync(dbContext);
                return;
            }
            catch (Exception exception) when (attempt < 10)
            {
                logger.LogWarning(exception, "Quote database startup attempt {Attempt} failed. Retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Quote database name is required.");
        }

        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
        checkCommand.Parameters.AddWithValue("databaseName", databaseName);
        var exists = await checkCommand.ExecuteScalarAsync() is not null;

        if (exists)
        {
            return;
        }

        logger.LogInformation("Creating quote database {DatabaseName}.", databaseName);
        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)}";
        await createCommand.ExecuteNonQueryAsync();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
