namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQueryResponse
{
    public StateStoreQueryItem[] Items { get; init; } = Array.Empty<StateStoreQueryItem>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? Token { get; init; }
}