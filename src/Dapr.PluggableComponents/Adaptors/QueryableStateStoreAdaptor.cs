using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.QueriableStateStore;

namespace Dapr.PluggableComponents.Adaptors;

/// <remarks>
/// .NET prefers the "queryable" spelling compared to the protos definition.
/// </remarks>
public class QueryableStateStoreAdaptor : QueriableStateStoreBase
{
    private readonly ILogger<QueryableStateStoreAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IQueryableStateStore> componentProvider;

    public QueryableStateStoreAdaptor(ILogger<QueryableStateStoreAdaptor> logger, IDaprPluggableComponentProvider<IQueryableStateStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<Proto.Components.V1.QueryResponse> Query(Proto.Components.V1.QueryRequest request, ServerCallContext context)
    {
        var response = await this.GetStateStore(context).QueryAsync(
            new Components.StateStore.StateStoreQueryRequest
            {
                Metadata = request.Metadata
            },
            context.CancellationToken);

        var grpcResponse = new Proto.Components.V1.QueryResponse
        {
            Token = response.Token
        };

        grpcResponse.Items.AddRange(
            response
                .Items
                .Select(item => new QueryItem
                {
                    ContentType = item.ContentType,
                    Data = ByteString.CopyFrom(item.Data ?? Array.Empty<byte>()),
                    Error = item.Error,
                    Etag = item.ETag != null ? new Etag { Value = item.ETag } : null,
                    Key = item.Key
                }));

        grpcResponse.Metadata.Add(response.Metadata);

        return grpcResponse;
    }

    private IQueryableStateStore GetStateStore(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}