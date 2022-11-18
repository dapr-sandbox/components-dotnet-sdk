namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreInitMetadata
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}
