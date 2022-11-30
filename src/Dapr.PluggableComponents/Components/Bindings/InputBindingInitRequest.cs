namespace Dapr.PluggableComponents.Components.Bindings;

// TODO: Consolidate init requests into single message?
public sealed class InputBindingInitRequest
{
    public MetadataRequest? Metadata { get; init; }
}
