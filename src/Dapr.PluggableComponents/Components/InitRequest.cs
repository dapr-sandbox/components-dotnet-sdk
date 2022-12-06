namespace Dapr.PluggableComponents.Components;

public sealed record InitRequest{
    public MetadataRequest? Metadata { get; init; }

    internal static InitRequest FromInitRequest(Proto.Components.V1.InitRequest request)
    {
        return new Components.InitRequest
        {
            Metadata = new Components.MetadataRequest { Properties = request.Metadata.Properties }
        };
    }
}
