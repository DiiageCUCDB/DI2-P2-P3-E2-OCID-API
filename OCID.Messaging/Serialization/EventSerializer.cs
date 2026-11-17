using System.Text.Json;

namespace OCID.Messaging.Serialization;

/// <summary>JSON (de)serialization shared by every provider, so the wire format stays identical when switching to Kafka.</summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static BinaryData Serialize<TEvent>(TEvent payload) => BinaryData.FromObjectAsJson(payload, Options);

    public static TEvent? Deserialize<TEvent>(BinaryData body) => body.ToObjectFromJson<TEvent>(Options);
}
