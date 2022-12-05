namespace Dapr.PluggableComponents.Components.PubSub;

public sealed record PubSubPullMessagesResponse(string TopicName, string MessageId)
{
    public byte[] Data { get; init; } = Array.Empty<byte>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? ContentType { get; init; }
}
