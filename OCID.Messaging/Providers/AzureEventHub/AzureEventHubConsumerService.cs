using System.Collections.Concurrent;
using System.Globalization;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OCID.Messaging.Configuration;
using OCID.Messaging.Models;
using OCID.Messaging.Subscriptions;

namespace OCID.Messaging.Providers.AzureEventHub;

/// <summary>
/// Runs one <see cref="EventProcessorClient"/> per (topic, consumer group) and dispatches every event to the
/// matching <see cref="EventSubscription"/>s. Checkpoints are stored in Blob Storage.
/// </summary>
/// <remarks>
/// Delivery is at-least-once: checkpoints are written every <see cref="AzureEventHubOptions.CheckpointInterval"/> events,
/// so after a crash a few events can be redelivered. A handler that throws is logged and the event is skipped
/// (no retry or dead-lettering yet), so handlers should be idempotent and catch what they can recover from.
/// </remarks>
public sealed class AzureEventHubConsumerService : BackgroundService
{
    private readonly MessagingOptions _options;
    private readonly IReadOnlyList<EventSubscription> _subscriptions;
    private readonly IServiceProvider _services;
    private readonly ILogger<AzureEventHubConsumerService> _logger;

    public AzureEventHubConsumerService(
        IOptions<MessagingOptions> options,
        IEnumerable<EventSubscription> subscriptions,
        IServiceProvider services,
        ILogger<AzureEventHubConsumerService> logger)
    {
        _options = options.Value;
        _subscriptions = subscriptions.ToList();
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_subscriptions.Count == 0)
        {
            return;
        }

        var hub = _options.AzureEventHub;
        if (string.IsNullOrWhiteSpace(hub.CheckpointStorageConnectionString))
        {
            throw new InvalidOperationException("Messaging:AzureEventHub:CheckpointStorageConnectionString is required when there are subscriptions.");
        }

        var checkpointContainer = new BlobContainerClient(hub.CheckpointStorageConnectionString, hub.CheckpointContainer);
        await checkpointContainer.CreateIfNotExistsAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

        var processors = new List<EventProcessorClient>();
        try
        {
            var groups = _subscriptions.GroupBy(s => (s.Topic, Group: s.ConsumerGroup ?? _options.DefaultConsumerGroup));

            foreach (var group in groups)
            {
                var subscriptions = group.ToList();
                var counters = new ConcurrentDictionary<string, int>();
                var processor = new EventProcessorClient(checkpointContainer, group.Key.Group, hub.ConnectionString, group.Key.Topic);

                processor.ProcessEventAsync += args => OnEventAsync(args, subscriptions, counters, hub.CheckpointInterval);
                processor.ProcessErrorAsync += args =>
                {
                    _logger.LogError(args.Exception, "Event Hubs error on {Topic} partition {Partition} during {Operation}",
                        group.Key.Topic, args.PartitionId, args.Operation);
                    return Task.CompletedTask;
                };

                await processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
                processors.Add(processor);
                _logger.LogInformation("Consuming {Topic} with consumer group {Group} ({Count} subscription(s))",
                    group.Key.Topic, group.Key.Group, subscriptions.Count);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // normal shutdown
        }
        finally
        {
            foreach (var processor in processors)
            {
                await processor.StopProcessingAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private async Task OnEventAsync(
        ProcessEventArgs args,
        IReadOnlyList<EventSubscription> subscriptions,
        ConcurrentDictionary<string, int> counters,
        int checkpointInterval)
    {
        if (!args.HasEvent)
        {
            return;
        }

        var message = ToRawMessage(args);

        foreach (var subscription in subscriptions.Where(s => s.Matches(message)))
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                await subscription.DispatchAsync(scope.ServiceProvider, message, args.CancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
            {
                return; // shutting down: leave the event un-checkpointed so it is redelivered
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler for {EventType} failed on {Topic} partition {Partition} (sequence {Sequence}); event skipped",
                    subscription.EventType, message.Topic, message.PartitionId, message.Offset);
            }
        }

        var partitionId = args.Partition.PartitionId;
        var processed = counters.AddOrUpdate(partitionId, 1, (_, current) => current + 1);
        if (processed >= checkpointInterval)
        {
            await args.UpdateCheckpointAsync(args.CancellationToken).ConfigureAwait(false);
            counters[partitionId] = 0;
        }
    }

    private static RawMessage ToRawMessage(ProcessEventArgs args)
    {
        var data = args.Data;
        var headers = data.Properties.ToDictionary(
            p => p.Key,
            p => Convert.ToString(p.Value, CultureInfo.InvariantCulture) ?? string.Empty);

        return new RawMessage(
            args.Partition.EventHubName,
            args.Partition.PartitionId,
            data.PartitionKey,
            data.EventBody,
            headers,
            data.EnqueuedTime,
            data.SequenceNumber.ToString(CultureInfo.InvariantCulture));
    }
}
