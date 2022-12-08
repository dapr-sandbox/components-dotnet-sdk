using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQueryRequest
{
    public StateStoreQuery? Query { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static StateStoreQueryRequest FromQueryRequest(QueryRequest request)
    {
        return new StateStoreQueryRequest
        {
            Query = StateStoreQuery.FromQuery(request.Query),
            Metadata = request.Metadata
        };
    }
}