namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQueryRequest
{
    public StateStoreQuery? Query { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}