using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkDeleteRequest
{
    public IReadOnlyList<StateStoreDeleteRequest> Items { get; init; } = new List<StateStoreDeleteRequest>();

    internal static StateStoreBulkDeleteRequest FromBulkDeleteRequest(BulkDeleteRequest request)
    {
        return new StateStoreBulkDeleteRequest
        {
            Items =
                request
                    .Items
                    .Select(StateStoreDeleteRequest.FromDeleteRequest)
                    .ToList()
        };
    }
}