namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkSetRequest
{
    public IReadOnlyList<StateStoreSetRequest> Items { get; init; } = new List<StateStoreSetRequest>();
}
