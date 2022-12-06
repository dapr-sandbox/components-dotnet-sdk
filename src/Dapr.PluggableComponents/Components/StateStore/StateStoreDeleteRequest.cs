using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreDeleteRequest(string Key)
{
    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public StateStoreStateOptions? Options { get; init; }

    internal static StateStoreDeleteRequest FromDeleteRequest(DeleteRequest request)
    {
        return new StateStoreDeleteRequest(request.Key)
        {
            ETag = request.Etag?.Value,
            Metadata = request.Metadata,
            Options = StateStoreStateOptions.FromStateOptions(request.Options)
        };
    }
}
