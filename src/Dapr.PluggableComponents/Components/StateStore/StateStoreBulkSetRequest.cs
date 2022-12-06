using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkSetRequest
{
    public IReadOnlyList<StateStoreSetRequest> Items { get; init; } = new List<StateStoreSetRequest>();

    internal static StateStoreBulkSetRequest FromBulkSetRequest(BulkSetRequest request)
    {
        return new StateStoreBulkSetRequest
        {
            Items =
                request
                    .Items
                    .Select(StateStoreSetRequest.FromSetRequest)
                    .ToList()
        };        
    }
}
