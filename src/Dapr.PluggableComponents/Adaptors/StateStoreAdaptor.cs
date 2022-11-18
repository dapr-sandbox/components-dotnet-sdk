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
    private readonly IStateStore store;

    public StateStoreAdaptor(ILogger<StateStoreAdaptor> logger, IStateStore store)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.store = store ?? throw new ArgumentNullException(nameof(store));
    }

    private static BulkStateItem ToBulkStateItem(StateStoreBulkStateItem item)
    {
        var bulkStateItem = new BulkStateItem
        {
            ContentType = item.ContentType,
            Data = ByteString.CopyFrom(item.Data),
            Error = item.Error
        };

        if (!String.IsNullOrEmpty(item.ETag))
        {
            bulkStateItem.Etag = new Etag { Value = item.ETag };
        }

        item.Metadata.CopyTo(bulkStateItem.Metadata);

        return bulkStateItem;
    }

    public override async Task<BulkGetResponse> BulkGet(BulkGetRequest request, ServerCallContext context)
    {
        this.logger.LogInformation("Bulk get request for {count} keys", request.Items.Count);

        var response = await this.store.BulkGetAsync(
            new StateStoreBulkGetRequest
            {
                Items =
                    request
                        .Items
                        .Select(
                            item => new StateStoreGetRequest
                            {
                                Key = item.Key,
                                Metadata = item.Metadata
                            })
                        .ToList()
            },
            context.CancellationToken);

        var items =
            response
                .Items
                .Select(ToBulkStateItem)
                .ToList();

        var bulkGetResponse = new BulkGetResponse
        {
            Got = items.Any()
        };

        bulkGetResponse.Items.AddRange(items);

        return bulkGetResponse;
    }

    public override async Task<BulkSetResponse> BulkSet(BulkSetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        await this.store.BulkSetAsync(
            new StateStoreBulkSetRequest
            {
                Items =
                    request
                        .Items
                        .Select(
                            item => new StateStoreSetRequest
                            {
                                ContentType = item.ContentType,
                                ETag = item.Etag?.Value ?? String.Empty,
                                Key = item.Key,
                                Metadata = item.Metadata,
                                Value = item.Value.Memory
                            })
                        .ToList()
            },
            ctx.CancellationToken).ConfigureAwait(false);

        return new BulkSetResponse();
    }

    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Features request");

        var response = new FeaturesResponse();

        if (this.store is IFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(ctx.CancellationToken).ConfigureAwait(false);
    
            response.Features.AddRange(featuresResponse);
        }

        return response;
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

    public async override Task<InitResponse> Init(InitRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Init request");
        
        await this.store.InitAsync(
            new StateStoreInitRequest
            {
                Metadata = new StateStoreInitMetadata { Properties = request.Metadata.Properties },
            },
            ctx.CancellationToken);
        
        return new InitResponse();
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Ping request");

        if (this.store is IPing ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    public override async Task<SetResponse> Set(SetRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Set request for key {key}", request.Key);

        await this.store.SetAsync(
            new StateStoreSetRequest
            {
                ContentType = request.ContentType,
                ETag = request.Etag?.Value ?? String.Empty,
                Key = request.Key,
                Metadata = request.Metadata,
                Value = request.Value.Memory
            },
            ctx.CancellationToken).ConfigureAwait(false);

        return new SetResponse();
    }
}
