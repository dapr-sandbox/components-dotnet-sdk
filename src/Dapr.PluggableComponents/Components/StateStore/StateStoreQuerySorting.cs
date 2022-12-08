using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public enum StateStoreQuerySortingOrder
{
    Ascending = 0,
    Descending = 1
}

public sealed record StateStoreQuerySorting(string Key)
{
    public StateStoreQuerySortingOrder Order { get; init; } = StateStoreQuerySortingOrder.Ascending;

    internal static StateStoreQuerySorting FromSorting(Sorting sorting)
        => new StateStoreQuerySorting(sorting.Key)
        {
            Order = (StateStoreQuerySortingOrder)sorting.Order
        };
}
