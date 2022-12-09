namespace Dapr.PluggableComponents.Components.StateStore;

public interface IBulkStateStore
{
    Task BulkDeleteAsync(StateStoreDeleteRequest[] requests, CancellationToken cancellationToken = default);

    Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreGetRequest[] requests, CancellationToken cancellationToken = default);

    Task BulkSetAsync(StateStoreSetRequest[] requests, CancellationToken cancellationToken = default);
}
