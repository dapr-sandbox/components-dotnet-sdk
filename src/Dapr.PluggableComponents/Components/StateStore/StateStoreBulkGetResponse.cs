namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreBulkStateItem
{
    public string ContentType { get; init; } = String.Empty;

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string Error { get; init; } = String.Empty;

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed class StateStoreBulkGetResponse
{
    public bool Got { get; init; }

    public IReadOnlyList<StateStoreBulkStateItem> Items { get; init; } = new List<StateStoreBulkStateItem>();
}