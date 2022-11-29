namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubPullMessagesResponse
{
    public byte[]? Data { get; init; } = Array.Empty<byte>();

    public string? TopicName { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? ContentType { get; init; }

    public string? Id { get; init; }
}
