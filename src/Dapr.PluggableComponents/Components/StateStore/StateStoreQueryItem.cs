namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQueryItem(string Key)
{
    public string? ContentType { get; init; }

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string? Error { get; init; }

    public string? ETag { get; init; }
}
