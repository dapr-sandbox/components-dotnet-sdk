namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkGetResponse(bool Got)
{
    public IReadOnlyList<StateStoreBulkStateItem> Items { get; init; } = new List<StateStoreBulkStateItem>();
}