namespace OCID.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>StackExchange.Redis connection string, e.g. "localhost:6379,password=secret,ssl=false".</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>Prefix prepended to every key, so several apps can share one Redis instance.</summary>
    public string KeyPrefix { get; set; } = "ocid:";

    /// <summary>Redis database index. -1 uses the default database from the connection string.</summary>
    public int Database { get; set; } = -1;

    /// <summary>Expiry applied when a call does not specify one. Null means keys never expire.</summary>
    public TimeSpan? DefaultExpiry { get; set; }

    public int ConnectTimeoutMs { get; set; } = 5000;
}
