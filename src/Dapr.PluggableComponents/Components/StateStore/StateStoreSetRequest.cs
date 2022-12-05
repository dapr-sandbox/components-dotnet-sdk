namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreSetRequest(string Key, ReadOnlyMemory<byte> Value)
{
    public string? ContentType { get; init; }

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public StateStoreStateOptions? Options { get; init; }
}
