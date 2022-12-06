using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreSetRequest(string Key, ReadOnlyMemory<byte> Value)
{
    public string? ContentType { get; init; }

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public StateStoreStateOptions? Options { get; init; }

    internal static StateStoreSetRequest FromSetRequest(SetRequest request)
    {
        return new StateStoreSetRequest(request.Key, request.Value.Memory)
        {
            ContentType = request.ContentType,
            ETag = request.Etag?.Value,
            Metadata = request.Metadata,
            Options = StateStoreStateOptions.FromStateOptions(request.Options)
        };
    }
}
