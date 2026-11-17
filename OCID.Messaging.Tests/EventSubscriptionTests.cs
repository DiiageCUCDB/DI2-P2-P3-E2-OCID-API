using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OCID.Messaging.Interfaces;
using OCID.Messaging.Models;
using OCID.Messaging.Providers;
using OCID.Messaging.Serialization;
using OCID.Messaging.Subscriptions;

namespace OCID.Messaging.Tests;

public class EventSubscriptionTests
{
    private sealed record UserCreated(string Name, int Age);

    private sealed class RecordingHandler : IEventHandler<UserCreated>
    {
        public List<ReceivedEvent<UserCreated>> Received { get; } = [];

        public Task HandleAsync(ReceivedEvent<UserCreated> message, CancellationToken cancellationToken)
        {
            Received.Add(message);
            return Task.CompletedTask;
        }
    }

    private static RawMessage CreateMessage(object payload, Dictionary<string, string>? headers = null) =>
        new(
            Topic: "users",
            PartitionId: "0",
            Key: "k1",
            Body: EventSerializer.Serialize(payload),
            Headers: headers ?? new Dictionary<string, string>(),
            EnqueuedAt: DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            Offset: "42");

    private static (ServiceProvider Services, RecordingHandler Handler) BuildServices()
    {
        var handler = new RecordingHandler();
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        return (services, handler);
    }

    [Fact]
    public void Serializer_RoundTrips()
    {
        var body = EventSerializer.Serialize(new UserCreated("Ada", 36));

        Assert.Equal(new UserCreated("Ada", 36), EventSerializer.Deserialize<UserCreated>(body));
    }

    [Fact]
    public async Task Dispatch_DeserializesPayload_AndFillsMetadata()
    {
        var (services, handler) = BuildServices();
        var subscription = new EventSubscription<UserCreated, RecordingHandler>("users");
        var message = CreateMessage(new UserCreated("Ada", 36), new Dictionary<string, string>
        {
            [MessageHeaders.EventId] = "evt-1",
            [MessageHeaders.EventType] = nameof(UserCreated),
            [MessageHeaders.CorrelationId] = "corr-1",
            [MessageHeaders.OccurredAt] = "2026-02-03T04:05:06.0000000+00:00"
        });

        await subscription.DispatchAsync(services, message, CancellationToken.None);

        var received = Assert.Single(handler.Received);
        Assert.Equal(new UserCreated("Ada", 36), received.Payload);
        Assert.Equal("evt-1", received.EventId);
        Assert.Equal("corr-1", received.CorrelationId);
        Assert.Equal(DateTimeOffset.Parse("2026-02-03T04:05:06Z"), received.OccurredAt);
        Assert.Equal("users", received.Topic);
        Assert.Equal("k1", received.Key);
        Assert.Equal("42", received.Offset);
    }

    [Fact]
    public async Task Dispatch_FallsBackToEnqueuedTime_WhenNoOccurredAtHeader()
    {
        var (services, handler) = BuildServices();
        var subscription = new EventSubscription<UserCreated, RecordingHandler>("users");

        await subscription.DispatchAsync(services, CreateMessage(new UserCreated("Ada", 36)), CancellationToken.None);

        Assert.Equal(DateTimeOffset.Parse("2026-01-01T00:00:00Z"), Assert.Single(handler.Received).OccurredAt);
    }

    [Fact]
    public void Matches_IsTrue_WhenEventTypeEqualsOrIsMissing()
    {
        var subscription = new EventSubscription<UserCreated, RecordingHandler>("users");

        Assert.True(subscription.Matches(CreateMessage(new object())));
        Assert.True(subscription.Matches(CreateMessage(new object(), new() { [MessageHeaders.EventType] = nameof(UserCreated) })));
    }

    [Fact]
    public void Matches_IsFalse_ForOtherEventType()
    {
        var subscription = new EventSubscription<UserCreated, RecordingHandler>("users");

        Assert.False(subscription.Matches(CreateMessage(new object(), new() { [MessageHeaders.EventType] = "UserDeleted" })));
    }

    [Fact]
    public void Matches_UsesCustomEventType()
    {
        var subscription = new EventSubscription<UserCreated, RecordingHandler>("users", eventType: "user.created");

        Assert.True(subscription.Matches(CreateMessage(new object(), new() { [MessageHeaders.EventType] = "user.created" })));
        Assert.False(subscription.Matches(CreateMessage(new object(), new() { [MessageHeaders.EventType] = nameof(UserCreated) })));
    }

    [Fact]
    public async Task NullPublisher_CompletesWithoutThrowing()
    {
        var publisher = new NullEventPublisher(NullLogger<NullEventPublisher>.Instance);

        await publisher.PublishAsync("users", new UserCreated("Ada", 36));
        await publisher.PublishBatchAsync("users", new[] { new UserCreated("Ada", 36) });
    }
}
