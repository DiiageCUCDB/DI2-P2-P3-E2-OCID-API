namespace OCID.Redis;

/// <summary>
/// Abstraction over Redis used by the rest of the application (cache, sessions,
/// authorization codes, refresh tokens, rate limiting...).
/// All keys are automatically namespaced with the configured key prefix.
/// </summary>
public interface IRedisClient
{
    /// <summary>Sends a PING and returns the round-trip latency.</summary>
    Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default);

    // ---------- Strings ----------

    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    /// <param name="expiry">Time to live. When null, <c>RedisOptions.DefaultExpiry</c> is used (no expiry if that is null too).</param>
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    // ---------- Typed values (JSON serialized) ----------

    /// <summary>Returns the deserialized value, or <c>default</c> when the key does not exist.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically reads and deletes a key (GETDEL, requires Redis 6.2+).
    /// Useful for single-use values such as authorization codes.
    /// </summary>
    Task<T?> GetAndDeleteAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Sets the value only if the key does not exist yet (SET NX). Returns true when written.</summary>
    Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>Returns the cached value, or calls <paramref name="factory"/>, stores its result and returns it.</summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    // ---------- Keys ----------

    /// <returns>true if the key existed and was removed.</returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <returns>true if the key exists and the expiry was applied.</returns>
    Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <returns>Remaining time to live, or null if the key does not exist or has no expiry.</returns>
    Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);

    // ---------- Counters ----------

    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    // ---------- Hashes ----------

    Task SetHashFieldAsync(string key, string field, string value, CancellationToken cancellationToken = default);

    Task<string?> GetHashFieldAsync(string key, string field, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetHashAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> RemoveHashFieldAsync(string key, string field, CancellationToken cancellationToken = default);
}
