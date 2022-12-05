namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkDeleteRequest
{
    public IReadOnlyList<StateStoreDeleteRequest> Items { get; init; } = new List<StateStoreDeleteRequest>();
}