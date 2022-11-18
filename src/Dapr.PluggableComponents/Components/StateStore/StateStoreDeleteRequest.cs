namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreDeleteRequest
{
    public string? ETag { get; init; }

    public string Key { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
