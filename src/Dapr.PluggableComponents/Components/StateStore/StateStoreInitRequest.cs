namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreInitRequest
{
    public StateStoreInitMetadata Metadata { get; init; } = new StateStoreInitMetadata();
}
