using System.Text.Json;
using StackExchange.Redis;

namespace quote_service.Services;

public sealed class RedisCacheService : IRedisCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string? connectionString;
    private readonly ILogger<RedisCacheService> logger;
    private readonly Lazy<ConnectionMultiplexer?> connection;

    public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
    {
        connectionString = configuration["Redis:ConnectionString"];
        this.logger = logger;
        connection = new Lazy<ConnectionMultiplexer?>(CreateConnection);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var database = GetDatabase();
            if (database is null)
            {
                return default;
            }

            var value = await database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis get failed for key {CacheKey}. Falling back to source of truth.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        try
        {
            var database = GetDatabase();
            if (database is null)
            {
                return;
            }

            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await database.StringSetAsync(key, payload, ttl);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis set failed for key {CacheKey}. Continuing without cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var database = GetDatabase();
            if (database is null)
            {
                return;
            }

            await database.KeyDeleteAsync(key);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis remove failed for key {CacheKey}. Continuing without cache.", key);
        }
    }

    private IDatabase? GetDatabase()
    {
        return connection.Value?.GetDatabase();
    }

    private ConnectionMultiplexer? CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("Redis connection string is not configured. Continuing without Redis cache.");
            return null;
        }

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 1;
            options.ConnectTimeout = 500;
            options.SyncTimeout = 500;
            options.AsyncTimeout = 500;
            return ConnectionMultiplexer.Connect(options);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis connection failed. Continuing without Redis cache.");
            return null;
        }
    }
}
