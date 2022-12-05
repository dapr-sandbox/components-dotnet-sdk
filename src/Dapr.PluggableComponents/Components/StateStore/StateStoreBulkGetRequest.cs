namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkGetRequest
{
    public IReadOnlyList<StateStoreGetRequest> Items { get; init; } = new List<StateStoreGetRequest>();
}