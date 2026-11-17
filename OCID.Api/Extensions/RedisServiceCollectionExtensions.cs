using Microsoft.Extensions.Options;
using OCID.Redis;
using StackExchange.Redis;

namespace OCID.Api.Extensions;

public static class RedisServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IConnectionMultiplexer"/> (singleton) and <see cref="IRedisClient"/>,
    /// configured from the "Redis" section of the configuration.
    /// </summary>
    public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Redis:ConnectionString must be set.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            var config = ConfigurationOptions.Parse(options.ConnectionString);
            config.AbortOnConnectFail = false; // keep retrying in the background instead of crashing at startup
            config.ConnectTimeout = options.ConnectTimeoutMs;
            return ConnectionMultiplexer.Connect(config);
        });

        services.AddSingleton<IRedisClient, RedisClient>();

        return services;
    }
}
