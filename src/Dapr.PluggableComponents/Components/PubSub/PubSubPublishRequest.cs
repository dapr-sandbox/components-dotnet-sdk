namespace Dapr.PluggableComponents.Components.PubSub;

public sealed record PubSubPublishRequest(string PubSubName, string Topic)
{
    public ReadOnlyMemory<byte> Data { get; init; } = ReadOnlyMemory<byte>.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? ContentType { get; init; }
}