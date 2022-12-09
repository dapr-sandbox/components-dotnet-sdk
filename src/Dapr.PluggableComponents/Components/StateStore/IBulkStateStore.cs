namespace Dapr.PluggableComponents.Components.StateStore;

public interface IBulkStateStore
{
    Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default);

    Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default);

    Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default);
}
