using Dapr.Proto.Components.V1;
using Google.Protobuf.WellKnownTypes;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQuery
{
    // TODO: Refactor as a more strongly-typed filter API.
    public IReadOnlyDictionary<string, Any> Filter { get; init; } = new Dictionary<string, Any>();

    public StateStoreQueryPagination? Pagination { get; init; }

    public StateStoreQuerySorting[] Sorting { get; init; } = Array.Empty<StateStoreQuerySorting>();

    internal static StateStoreQuery? FromQuery(Query? query)
    {
        return query != null
            ? new StateStoreQuery
            {
                Filter = query.Filter,
                Pagination = StateStoreQueryPagination.FromPagination(query.Pagination),
                Sorting = query.Sort.Select(StateStoreQuerySorting.FromSorting).ToArray()
            }
            : null;
    }
}
