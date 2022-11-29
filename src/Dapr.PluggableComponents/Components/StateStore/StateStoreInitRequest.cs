namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreInitRequest
{
    public MetadataRequest Metadata { get; init; } = new MetadataRequest();
}
