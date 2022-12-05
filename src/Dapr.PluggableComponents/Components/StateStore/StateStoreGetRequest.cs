namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreGetRequest(string Key)
{
    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
