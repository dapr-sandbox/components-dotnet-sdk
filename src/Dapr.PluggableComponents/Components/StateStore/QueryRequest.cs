namespace Dapr.PluggableComponents.Components.StateStore;

public enum QueryRequestSortingOrder
{
    Ascending = 0,
    Descending = 1
}

public sealed class QueryRequestSorting
{
    public QueryRequestSortingOrder Order { get; init; }
}

public sealed class QueryRequestPagination
{
    public long Limit { get; init; }

    public string Token { get; init; }
}

public sealed class QueryRequestQuery
{
    public IReadOnlyDictionary<string, object> Filter { get; init; } = new Dictionary<string, object>();

    public QueryRequestPagination? Pagination { get; init; }

    public QueryRequestSorting? Sorting { get; init; }
}

public sealed class QueryRequest
{
    public QueryRequestQuery? Query { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}