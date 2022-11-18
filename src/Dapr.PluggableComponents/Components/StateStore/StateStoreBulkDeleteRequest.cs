namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreBulkDeleteRequest
{
    public IReadOnlyList<StateStoreDeleteRequest> Items { get; init; } = new List<StateStoreDeleteRequest>();
}