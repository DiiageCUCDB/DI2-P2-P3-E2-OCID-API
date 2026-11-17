using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OCID.Redis;

public sealed class RedisClient : IRedisClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connection;
    private readonly RedisOptions _options;

    public RedisClient(IConnectionMultiplexer connection, IOptions<RedisOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    private IDatabase Db => _connection.GetDatabase(_options.Database);

    private RedisKey BuildKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return (RedisKey)(_options.KeyPrefix + key);
    }

    private TimeSpan? ResolveExpiry(TimeSpan? expiry) => expiry ?? _options.DefaultExpiry;

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static T? Deserialize<T>(RedisValue value) =>
        value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!, JsonOptions);

    public async Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default) =>
        await Db.PingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    // ---------- Strings ----------

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        return value.IsNull ? null : (string?)value;
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) =>
        await Db.StringSetAsync(BuildKey(key), value, ResolveExpiry(expiry), When.Always)
            .WaitAsync(cancellationToken).ConfigureAwait(false);

    // ---------- Typed values ----------

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Deserialize<T>(value);
    }

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) =>
        SetStringAsync(key, Serialize(value), expiry, cancellationToken);

    public async Task<T?> GetAndDeleteAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetDeleteAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Deserialize<T>(value);
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) =>
        await Db.StringSetAsync(BuildKey(key), Serialize(value), ResolveExpiry(expiry), When.NotExists)
            .WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await Db.StringGetAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!cached.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>((string)cached!, JsonOptions)!;
        }

        var created = await factory().ConfigureAwait(false);
        await SetAsync(key, created, expiry, cancellationToken).ConfigureAwait(false);
        return created;
    }

    // ---------- Keys ----------

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        await Db.KeyDeleteAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        await Db.KeyExistsAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default) =>
        await Db.KeyExpireAsync(BuildKey(key), expiry).WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default) =>
        await Db.KeyTimeToLiveAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);

    // ---------- Counters ----------

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default) =>
        await Db.StringIncrementAsync(BuildKey(key), value).WaitAsync(cancellationToken).ConfigureAwait(false);

    // ---------- Hashes ----------

    public async Task SetHashFieldAsync(string key, string field, string value, CancellationToken cancellationToken = default) =>
        await Db.HashSetAsync(BuildKey(key), field, value).WaitAsync(cancellationToken).ConfigureAwait(false);

    public async Task<string?> GetHashFieldAsync(string key, string field, CancellationToken cancellationToken = default)
    {
        var value = await Db.HashGetAsync(BuildKey(key), field).WaitAsync(cancellationToken).ConfigureAwait(false);
        return value.IsNull ? null : (string?)value;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetHashAsync(string key, CancellationToken cancellationToken = default)
    {
        var entries = await Db.HashGetAllAsync(BuildKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        return entries.ToDictionary(e => (string)e.Name!, e => (string)e.Value!);
    }

    public async Task<bool> RemoveHashFieldAsync(string key, string field, CancellationToken cancellationToken = default) =>
        await Db.HashDeleteAsync(BuildKey(key), field).WaitAsync(cancellationToken).ConfigureAwait(false);
}
