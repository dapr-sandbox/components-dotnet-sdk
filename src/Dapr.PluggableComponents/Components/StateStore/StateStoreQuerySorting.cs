namespace Dapr.PluggableComponents.Components.StateStore;

public enum StateStoreQuerySortingOrder
{
    Ascending = 0,
    Descending = 1
}

public sealed record StateStoreQuerySorting
{
    public string Key { get; init; } = String.Empty;

    public StateStoreQuerySortingOrder Order { get; init; } = StateStoreQuerySortingOrder.Ascending;
}
