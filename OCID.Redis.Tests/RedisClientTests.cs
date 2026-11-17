using Microsoft.Extensions.Options;
using NSubstitute;
using OCID.Redis;
using StackExchange.Redis;

namespace OCID.Redis.Tests;

public class RedisClientTests
{
    private readonly IDatabase _db = Substitute.For<IDatabase>();
    private readonly RedisOptions _options = new() { KeyPrefix = "test:" };

    private RedisClient CreateSut()
    {
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_db);
        return new RedisClient(connection, Options.Create(_options));
    }

    private sealed record Sample(string Name, int Age);

    [Fact]
    public async Task GetStringAsync_PrefixesKey_AndReturnsValue()
    {
        _db.StringGetAsync((RedisKey)"test:foo", Arg.Any<CommandFlags>())
            .Returns(Task.FromResult((RedisValue)"bar"));

        var result = await CreateSut().GetStringAsync("foo");

        Assert.Equal("bar", result);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenKeyMissing()
    {
        _db.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(RedisValue.Null));

        Assert.Null(await CreateSut().GetStringAsync("missing"));
    }

    [Fact]
    public async Task GetAsync_DeserializesJson()
    {
        _db.StringGetAsync((RedisKey)"test:user", Arg.Any<CommandFlags>())
            .Returns(Task.FromResult((RedisValue)"{\"name\":\"Ada\",\"age\":36}"));

        var result = await CreateSut().GetAsync<Sample>("user");

        Assert.Equal(new Sample("Ada", 36), result);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenKeyMissing()
    {
        _db.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(RedisValue.Null));

        Assert.Null(await CreateSut().GetAsync<Sample>("missing"));
    }

    [Fact]
    public async Task SetStringAsync_UsesDefaultExpiry_WhenNoneGiven()
    {
        _options.DefaultExpiry = TimeSpan.FromMinutes(5);
        _db.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>())
            .Returns(Task.FromResult(true));

        await CreateSut().SetStringAsync("k", "v");

        await _db.Received(1).StringSetAsync((RedisKey)"test:k", "v", TimeSpan.FromMinutes(5), When.Always);
    }

    [Fact]
    public async Task SetStringAsync_ExplicitExpiry_OverridesDefault()
    {
        _options.DefaultExpiry = TimeSpan.FromMinutes(5);
        _db.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>())
            .Returns(Task.FromResult(true));

        await CreateSut().SetStringAsync("k", "v", TimeSpan.FromSeconds(30));

        await _db.Received(1).StringSetAsync((RedisKey)"test:k", "v", TimeSpan.FromSeconds(30), When.Always);
    }

    [Fact]
    public async Task SetIfNotExistsAsync_UsesNotExistsCondition()
    {
        _db.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>())
            .Returns(Task.FromResult(true));

        var written = await CreateSut().SetIfNotExistsAsync("lock", "x", TimeSpan.FromSeconds(10));

        Assert.True(written);
        await _db.Received(1).StringSetAsync((RedisKey)"test:lock", Arg.Any<RedisValue>(), TimeSpan.FromSeconds(10), When.NotExists);
    }

    [Fact]
    public async Task GetOrSetAsync_DoesNotCallFactory_WhenCached()
    {
        _db.StringGetAsync((RedisKey)"test:user", Arg.Any<CommandFlags>())
            .Returns(Task.FromResult((RedisValue)"{\"name\":\"Ada\",\"age\":36}"));
        var called = false;

        var result = await CreateSut().GetOrSetAsync("user", () =>
        {
            called = true;
            return Task.FromResult(new Sample("Other", 1));
        });

        Assert.False(called);
        Assert.Equal(new Sample("Ada", 36), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyKey_Throws(string key)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => CreateSut().GetStringAsync(key));
    }
}
