using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreGetResponse
{
    public string? ContentType { get; init; }

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static GetResponse ToGetResponse(StateStoreGetResponse? response)
    {
        var grpcResponse = new GetResponse();

        // NOTE: in case of not found, you should not return any error.

        if (response != null)
        {
            grpcResponse.Data = ByteString.CopyFrom(response.Data);
            grpcResponse.Etag = response.ETag != null ? new Etag { Value = response.ETag } : null;
            
            grpcResponse.Metadata.Add(response.Metadata);
        }
        
        return grpcResponse;
    }
}
