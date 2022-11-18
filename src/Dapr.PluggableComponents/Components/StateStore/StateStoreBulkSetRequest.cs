namespace Dapr.PluggableComponents.Components.StateStore;

public class StateStoreBulkSetRequest
{
    public IReadOnlyList<StateStoreSetRequest> Items { get; init; } = new List<StateStoreSetRequest>();
}
