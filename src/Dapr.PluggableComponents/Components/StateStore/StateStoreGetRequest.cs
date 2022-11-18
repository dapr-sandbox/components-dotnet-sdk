namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreGetRequest
{
    public string Key { get; init; } = String.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
