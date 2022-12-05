namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubPublishRequest
{
    public ReadOnlyMemory<byte> Data { get; init; } = ReadOnlyMemory<byte>.Empty;

    public string PubSubName { get; init; } = String.Empty;

    public string Topic { get; init; } = String.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? ContentType { get; init; }
}