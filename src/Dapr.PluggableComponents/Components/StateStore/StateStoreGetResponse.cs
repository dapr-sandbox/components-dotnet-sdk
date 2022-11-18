namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreGetResponse
{
    public string ContentType { get; init; } = String.Empty;

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
