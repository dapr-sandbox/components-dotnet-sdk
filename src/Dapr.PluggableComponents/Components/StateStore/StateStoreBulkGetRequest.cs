using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkGetRequest
{
    public IReadOnlyList<StateStoreGetRequest> Items { get; init; } = new List<StateStoreGetRequest>();

    internal static StateStoreBulkGetRequest FromBulkGetRequest(BulkGetRequest request)
    {
        return new StateStoreBulkGetRequest
        {
            Items =
                request
                    .Items
                    .Select(StateStoreGetRequest.FromGetRequest)
                    .ToList()
        };
    }
}