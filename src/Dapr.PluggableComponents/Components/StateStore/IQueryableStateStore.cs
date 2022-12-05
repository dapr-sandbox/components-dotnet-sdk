namespace Dapr.PluggableComponents.Components.StateStore;

public interface IQueryableStateStore
{
    Task<StateStoreQueryResponse> QueryAsync(StateStoreQueryRequest request, CancellationToken cancellationToken = default);
}
