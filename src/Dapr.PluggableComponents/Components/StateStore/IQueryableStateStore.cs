namespace Dapr.PluggableComponents.Components.StateStore;

public interface IQueryableStateStore
{
    Task<QueryResponse> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
}
