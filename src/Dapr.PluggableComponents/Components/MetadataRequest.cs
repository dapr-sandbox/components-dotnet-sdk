namespace Dapr.PluggableComponents.Components;

public sealed record MetadataRequest
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();

    internal static MetadataRequest FromMetadataRequest(Dapr.Client.Autogen.Grpc.v1.MetadataRequest? request)
    {
        var metadataRequest = new MetadataRequest();

        if (request != null)
        {
            metadataRequest = metadataRequest with { Properties = request.Properties };
        }

        return metadataRequest;
    }
}
