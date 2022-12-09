using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
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
    private readonly IDaprPluggableComponentProvider<IStateStore> componentProvider;

    public StateStoreAdaptor(ILogger<StateStoreAdaptor> logger, IDaprPluggableComponentProvider<IStateStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<BulkDeleteResponse> BulkDelete(BulkDeleteRequest request, ServerCallContext context)
    {
        this.logger.LogInformation("BulkDelete request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(context.RequestHeaders);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            await bulkStateStore.BulkDeleteAsync(
                request.Items.Select(StateStoreDeleteRequest.FromDeleteRequest).ToArray(),
                context.CancellationToken);
        }
        else
        {
            this.logger.LogInformation("Store does not support bulk operations; falling back to individual operations.");

            foreach (var item in request.Items)
            {
                await stateStore.DeleteAsync(StateStoreDeleteRequest.FromDeleteRequest(item), context.CancellationToken);
            }
        }

        return new BulkDeleteResponse();
    }

    public override async Task<BulkGetResponse> BulkGet(BulkGetRequest request, ServerCallContext context)
    {
        this.logger.LogInformation("Bulk get request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(context.RequestHeaders);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            var response = await bulkStateStore.BulkGetAsync(
                request.Items.Select(StateStoreGetRequest.FromGetRequest).ToArray(),
                context.CancellationToken);
            
            return StateStoreBulkGetResponse.ToBulkGetResponse(response);
        }
        else
        {
            this.logger.LogInformation("Store does not support bulk operations; falling back to individual operations.");

            var responses = new List<BulkStateItem>();

            foreach (var item in request.Items)
            {
                var response = await stateStore.GetAsync(StateStoreGetRequest.FromGetRequest(item), context.CancellationToken);

                var stateItem = new BulkStateItem
                {
                    Key = item.Key
                };

                if (response != null)
                {
                    stateItem.ContentType = response.ContentType;
                    stateItem.Data = ByteString.CopyFrom(response.Data);
                    stateItem.Etag = response.ETag != null ? new Etag { Value = response.ETag } : null;
                    
                    stateItem.Metadata.Add(response.Metadata);
                }
                else
                {
                    stateItem.Error = "Unable to fetch the item.";
                }

                responses.Add(stateItem);        
            }

            var grpcResponse = new BulkGetResponse
            {
                Got = responses.Any(item => String.IsNullOrEmpty(item.Error))
            };

            grpcResponse.Items.Add(responses);

            return grpcResponse;
        }
    }

    public override async Task<BulkSetResponse> BulkSet(BulkSetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(ctx.RequestHeaders);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            await bulkStateStore.BulkSetAsync(
                request.Items.Select(StateStoreSetRequest.FromSetRequest).ToArray(),
                ctx.CancellationToken);
        }
        else
        {
            this.logger.LogInformation("Store does not support bulk operations; falling back to individual operations.");

            foreach (var item in request.Items)
            {
                await stateStore.SetAsync(StateStoreSetRequest.FromSetRequest(item), ctx.CancellationToken);
            }
        }

        return new BulkSetResponse();
    }

    public override async Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
    {
        this.logger.LogInformation("Delete request for key {key}", request.Key);

        await this.GetStateStore(context.RequestHeaders).DeleteAsync(
            StateStoreDeleteRequest.FromDeleteRequest(request),
            context.CancellationToken);

        return new DeleteResponse();
    }

    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Features request");

        var response = new FeaturesResponse();

        if (this.GetStateStore(ctx.RequestHeaders) is IPluggableComponentFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(ctx.CancellationToken);
    
            response.Features.AddRange(featuresResponse);
        }

        return response;
    }

    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Get request for key {key}", request.Key);

        var response = await this.GetStateStore(ctx.RequestHeaders).GetAsync(
            StateStoreGetRequest.FromGetRequest(request),
            ctx.CancellationToken);

        return StateStoreGetResponse.ToGetResponse(response);
    }

    public async override Task<InitResponse> Init(Proto.Components.V1.InitRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Init request");
        
        await this.GetStateStore(ctx.RequestHeaders).InitAsync(
            Components.MetadataRequest.FromMetadataRequest(request.Metadata),
            ctx.CancellationToken);
        
        return new InitResponse();
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Ping request");

        if (this.GetStateStore(ctx.RequestHeaders) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(ctx.CancellationToken);
        }

        return new PingResponse();
    }

    public override async Task<SetResponse> Set(SetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Set request for key {key}", request.Key);

        await this.GetStateStore(ctx.RequestHeaders).SetAsync(
            StateStoreSetRequest.FromSetRequest(request),
            ctx.CancellationToken);

        return new SetResponse();
    }

    private IStateStore GetStateStore(Metadata metadata)
    {
        return this.componentProvider.GetComponent(key => metadata.Get(key));
    }
}
