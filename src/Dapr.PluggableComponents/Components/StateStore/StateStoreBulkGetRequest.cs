namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreBulkGetRequest
{
    public IReadOnlyList<StateStoreGetRequest> Items { get; init; } = new List<StateStoreGetRequest>();
}