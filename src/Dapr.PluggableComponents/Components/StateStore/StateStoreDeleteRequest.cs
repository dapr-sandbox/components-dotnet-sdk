namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreDeleteRequest(string Key)
{
    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public StateStoreStateOptions? Options { get; init; }
}
