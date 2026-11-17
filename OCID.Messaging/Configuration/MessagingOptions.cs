namespace OCID.Messaging.Configuration;

public enum MessagingProvider
{
    AzureEventHub,

    /// <summary>Reserved: not implemented yet.</summary>
    Kafka
}

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>
    /// When false, a no-op publisher is registered and no consumer is started
    /// (local development, tests, environments without a broker).
    /// </summary>
    public bool Enabled { get; set; }

    public MessagingProvider Provider { get; set; } = MessagingProvider.AzureEventHub;

    /// <summary>Consumer group used by subscriptions that do not specify one.</summary>
    public string DefaultConsumerGroup { get; set; } = "$Default";

    public AzureEventHubOptions AzureEventHub { get; set; } = new();
}

public sealed class AzureEventHubOptions
{
    /// <summary>Event Hubs namespace connection string (without <c>EntityPath</c>: the hub name is the topic).</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Blob Storage connection string where consumer checkpoints are stored (only needed when there are subscriptions).</summary>
    public string CheckpointStorageConnectionString { get; set; } = string.Empty;

    public string CheckpointContainer { get; set; } = "ocid-checkpoints";

    /// <summary>A checkpoint is written every N processed events per partition.</summary>
    public int CheckpointInterval { get; set; } = 25;
}
