using System.Collections.Concurrent;
using System.Globalization;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using OCID.Messaging.Configuration;
using OCID.Messaging.Interfaces;
using OCID.Messaging.Models;
using OCID.Messaging.Serialization;

namespace OCID.Messaging.Providers.AzureEventHub;

public sealed class AzureEventHubPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly AzureEventHubOptions _options;
    private readonly ConcurrentDictionary<string, EventHubProducerClient> _producers = new();

    public AzureEventHubPublisher(IOptions<MessagingOptions> options)
    {
        _options = options.Value.AzureEventHub;
    }

    // One producer (and AMQP connection) per Event Hub, created lazily and reused.
    private EventHubProducerClient GetProducer(string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        return _producers.GetOrAdd(topic, t => new EventHubProducerClient(_options.ConnectionString, t));
    }

    public async Task PublishAsync<TEvent>(string topic, TEvent payload, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        var data = CreateEventData(payload, options);
        await GetProducer(topic)
            .SendAsync([data], new SendEventOptions { PartitionKey = options?.Key }, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PublishBatchAsync<TEvent>(string topic, IEnumerable<TEvent> payloads, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payloads);

        var producer = GetProducer(topic);
        var batchOptions = new CreateBatchOptions { PartitionKey = options?.Key };
        var batch = await producer.CreateBatchAsync(batchOptions, cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var payload in payloads)
            {
                var data = CreateEventData(payload, options);

                if (batch.TryAdd(data))
                {
                    continue;
                }

                if (batch.Count == 0)
                {
                    throw new InvalidOperationException("The event is too large to fit in an Event Hubs batch.");
                }

                // Batch is full: send it and start a new one.
                await producer.SendAsync(batch, cancellationToken).ConfigureAwait(false);
                batch.Dispose();
                batch = await producer.CreateBatchAsync(batchOptions, cancellationToken).ConfigureAwait(false);

                if (!batch.TryAdd(data))
                {
                    throw new InvalidOperationException("The event is too large to fit in an Event Hubs batch.");
                }
            }

            if (batch.Count > 0)
            {
                await producer.SendAsync(batch, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            batch.Dispose();
        }
    }

    private static EventData CreateEventData<TEvent>(TEvent payload, PublishOptions? options)
    {
        var data = new EventData(EventSerializer.Serialize(payload)) { ContentType = "application/json" };

        if (options?.Properties is { } extra)
        {
            foreach (var (name, value) in extra)
            {
                data.Properties[name] = value;
            }
        }

        data.Properties[MessageHeaders.EventId] = Guid.NewGuid().ToString();
        data.Properties[MessageHeaders.EventType] = options?.EventType ?? typeof(TEvent).Name;
        data.Properties[MessageHeaders.OccurredAt] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        if (options?.CorrelationId is { } correlationId)
        {
            data.Properties[MessageHeaders.CorrelationId] = correlationId;
        }

        return data;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var producer in _producers.Values)
        {
            await producer.DisposeAsync().ConfigureAwait(false);
        }

        _producers.Clear();
    }
}
