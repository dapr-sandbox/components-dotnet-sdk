using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreGetRequest(string Key)
{
    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static StateStoreGetRequest FromGetRequest(GetRequest request)
    {
        return new StateStoreGetRequest(request.Key)
        {
            Consistency = (StateStoreConsistency)request.Consistency,
            Metadata = request.Metadata
        };
    }
}
