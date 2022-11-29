namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubInitRequest
{
    public MetadataRequest Metadata { get; init; } = new MetadataRequest();
}
