using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.StateStore;

namespace Dapr.PluggableComponents.Adaptors;

public class StateStoreAdaptor : StateStoreBase
{
    private readonly ILogger<StateStoreAdaptor> logger;
    private readonly IStateStore store;

    
    public StateStoreAdaptor(ILogger<StateStoreAdaptor> logger, IStateStore store)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public override Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext ctx)
    {
        return Task.FromResult(new FeaturesResponse { });
    }

    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Get request for key {key}", request.Key);

        var response = await this.store.GetAsync(
            new StateStoreGetRequest
            {
                Key = request.Key,
                Metadata = request.Metadata
            },
            ctx.CancellationToken).ConfigureAwait(false);

        var grpcResponse = new GetResponse();

        if (response != null)
        {
            grpcResponse.Data = ByteString.CopyFrom(response.Data);
            grpcResponse.Etag = new Etag { Value = response.ETag };
            
            response.Metadata.CopyTo(grpcResponse.Metadata);
        }
        // in case of not found you should not return any error.

        return grpcResponse;
    }

    public override async Task<SetResponse> Set(SetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Set request for key {key}", request.Key);

        await this.store.SetAsync(
            new StateStoreSetRequest
            {
                ContentType = request.ContentType,
                ETag = request.Etag?.Value,
                Key = request.Key,
                Value = request.Value.Memory,
                Metadata = request.Metadata
            },
            ctx.CancellationToken).ConfigureAwait(false);

        return new SetResponse();
    }

    public override async Task<BulkSetResponse> BulkSet(BulkSetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        await this.store.BulkSetAsync(
            new StateStoreBulkSetRequest
            {
                Items = request.Items.Select(item => new StateStoreSetRequest
                {
                    ContentType = item.ContentType,
                    ETag = item.Etag?.Value,
                    Key = item.Key,
                    Value = item.Value.Memory,
                    Metadata = item.Metadata
                }).ToList()
            },
            ctx.CancellationToken).ConfigureAwait(false);

        return new BulkSetResponse();
    }

    public async override Task<InitResponse> Init(InitRequest request, ServerCallContext ctx)
    {
        logger.LogInformation("Init request for memstore");
        
        await this.store.InitAsync(
            new StateStoreInitMetadata
            {
                Properties = request.Metadata.Properties,
            },
            ctx.CancellationToken);
        
        return new InitResponse();
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        return Task.FromResult(new PingResponse());
    }
}
