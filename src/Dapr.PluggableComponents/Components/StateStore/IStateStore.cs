namespace Dapr.PluggableComponents.Components.StateStore;

public class StateStoreInitMetadata
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}

public class StateStoreGetRequest
{
    public string Key { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public class StateStoreGetResponse
{
    public string? ContentType { get; init; }

    public byte[] Data { get; init; }

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public class StateStoreSetRequest
{
    public string? ContentType { get; init; }

    public string? ETag { get; init; }

    public string Key { get; init; }

    public ReadOnlyMemory<byte> Value { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public class StateStoreBulkSetRequest
{
    public IReadOnlyList<StateStoreSetRequest> Items { get; init; } = new List<StateStoreSetRequest>();
}

public interface IStateStore
{
    Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default);

    Task InitAsync(StateStoreInitMetadata metadata, CancellationToken cancellationToken = default);

    Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default);

    Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default);
}
