namespace Dapr.PluggableComponents.Components.StateStore;

public interface IStateStore : IPluggableComponent
{
    Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default);

    Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default);

    Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default);

    Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default);

    Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default);
}
