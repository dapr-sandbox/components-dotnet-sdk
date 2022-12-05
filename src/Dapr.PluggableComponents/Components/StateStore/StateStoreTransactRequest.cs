namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreTransactRequest
{
    public StateStoreTransactOperation[] Operations { get; init; } = Array.Empty<StateStoreTransactOperation>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}