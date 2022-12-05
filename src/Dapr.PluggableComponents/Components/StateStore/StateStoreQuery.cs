namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQuery
{
    public IReadOnlyDictionary<string, object> Filter { get; init; } = new Dictionary<string, object>();

    public QueryRequestPagination? Pagination { get; init; }

    public StateStoreQuerySorting? Sorting { get; init; }
}
