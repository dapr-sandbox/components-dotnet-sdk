namespace Dapr.PluggableComponents.Components.StateStore;

// TODO: Consider IFeatures and IPing as inherited.
public interface IStateStore
{
    Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default);

    Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default);

    Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default);

    Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default);

    Task InitAsync(StateStoreInitRequest request, CancellationToken cancellationToken = default);

    Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default);
}
