namespace OCID.Messaging.Models;

/// <summary>Names of the headers/properties written on every published event (same names for every provider).</summary>
public static class MessageHeaders
{
    public const string EventId = "ocid-event-id";
    public const string EventType = "ocid-event-type";
    public const string CorrelationId = "ocid-correlation-id";
    public const string OccurredAt = "ocid-occurred-at";
}
