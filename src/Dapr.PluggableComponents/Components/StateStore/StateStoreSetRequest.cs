namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreSetRequest
{
    public string ContentType { get; init; } = String.Empty;

    public string? ETag { get; init; }

    public string Key { get; init; } = String.Empty;

    public ReadOnlyMemory<byte> Value { get; init; } = ReadOnlyMemory<byte>.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
