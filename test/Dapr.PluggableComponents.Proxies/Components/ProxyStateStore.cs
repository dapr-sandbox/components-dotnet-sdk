using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Proxies.Grpc.v1;
using Dapr.PluggableComponents.Proxies.Utilities;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Proxies.Components;

internal sealed class ProxyStateStore : IStateStore, IPluggableComponentFeatures, IPluggableComponentLiveness
{
    private readonly IGrpcChannelProvider grpcChannelProvider;
    private readonly ILogger<ProxyStateStore> logger;

    public ProxyStateStore(IGrpcChannelProvider grpcChannelProvider, ILogger<ProxyStateStore> logger)
    {
        this.grpcChannelProvider = grpcChannelProvider ?? throw new ArgumentNullException(nameof(grpcChannelProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IStateStore Members

    public async Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        var grpcRequest = new BulkDeleteRequest();

        grpcRequest.Items.AddRange(
            request.Items.Select(ToDeleteRequest));

        await this.GetClient().BulkDeleteAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        var grpcRequest = new BulkGetRequest();

        grpcRequest.Items.Add(request.Items.Select(ToGetRequest));

        var grpcResponse = await this.GetClient().BulkGetAsync(
            grpcRequest,
            cancellationToken: cancellationToken);

        return new StateStoreBulkGetResponse(grpcResponse.Got)
        {
            Items =
                grpcResponse
                    .Items
                    .Select(
                        item =>
                        {
                            return new StateStoreBulkStateItem(item.Key)
                            {
                                ContentType = item.ContentType ?? String.Empty,
                                Data = item.Data.Memory.ToArray(),
                                ETag = item.Etag?.Value,
                                Error = item.Error,
                                Metadata = item.Metadata
                            };
                        })
                    .ToList()
        };
    }

    public async Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        var grpcRequest = new BulkSetRequest();

        grpcRequest.Items.Add(
            request
                .Items
                .Select(ToSetRequest));

        await this.GetClient().BulkSetAsync(
            grpcRequest,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Delete request for key {key}", request.Key);

        await this.GetClient().DeleteAsync(
            ToDeleteRequest(request),
            cancellationToken: cancellationToken);
    }

    public async Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get request for key {key}", request.Key);

        var response = await this.GetClient().GetAsync(
            ToGetRequest(request),
            cancellationToken: cancellationToken);

        // TODO: Should response really be nullable?
        var storeResponse = new StateStoreGetResponse
        {
            ContentType = response.ContentType ?? String.Empty,
            Data = response.Data.Memory.ToArray(),
            ETag = response.Etag?.Value,
            Metadata = response.Metadata
        };

        return storeResponse;
    }

    public async Task InitAsync(Dapr.PluggableComponents.Components.MetadataRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Init request");

        var grpcRequest = new Dapr.PluggableComponents.Proxies.Grpc.v1.InitRequest
        {
            Metadata = new Dapr.PluggableComponents.Proxies.Grpc.v1.MetadataRequest()
        };

        grpcRequest.Metadata.Properties.Add(request.Properties);

        await this.GetClient().InitAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Set request for key {key}", request.Key);

        await this.GetClient().SetAsync(
            ToSetRequest(request),
            cancellationToken: cancellationToken);
    }

    #endregion

    #region IPluggableComponentFeatures Members

    public async Task<string[]> GetFeaturesAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get features request");

        var response = await this.GetClient().FeaturesAsync(
            new FeaturesRequest(),
            cancellationToken: cancellationToken);

        var features = response.Features.ToArray();

        this.logger.LogInformation("Returning features: {0}", String.Join(",", features));

        return features;
    }


    #endregion

    #region IPluggableComponentLiveness Members

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Ping request");

        await this.GetClient().PingAsync(
            new PingRequest(),
            cancellationToken: cancellationToken);
    }

    #endregion

    private StateStore.StateStoreClient GetClient()
    {
        return new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());
    }

    private static Etag? ToEtag(string? eTag)
    {
        return eTag != null ? new Etag { Value = eTag } : null;
    }

    private static StateOptions? ToStateOptions(StateStoreStateOptions? options)
    {
        return options != null
            ? new StateOptions
            {
                Concurrency = (Dapr.PluggableComponents.Proxies.Grpc.v1.StateOptions.Types.StateConcurrency)options.Concurrency,
                Consistency = (Dapr.PluggableComponents.Proxies.Grpc.v1.StateOptions.Types.StateConsistency)options.Consistency
            }
            : null;
    }

    private static DeleteRequest ToDeleteRequest(StateStoreDeleteRequest request)
    {
        var grpcRequest = new DeleteRequest
        {
            Etag = ToEtag(request.ETag),
            Key = request.Key,
            Options = ToStateOptions(request.Options)
        };

        grpcRequest.Metadata.Add(request.Metadata);

        return grpcRequest;
    }

    private static GetRequest ToGetRequest(StateStoreGetRequest request)
    {
        var grpcRequest = new GetRequest
        {
            Consistency = (Dapr.PluggableComponents.Proxies.Grpc.v1.StateOptions.Types.StateConsistency)request.Consistency,
            Key = request.Key            
        };

        grpcRequest.Metadata.Add(request.Metadata);

        return grpcRequest;
    }

    private static SetRequest ToSetRequest(StateStoreSetRequest request)
    {
        var grpcRequest = new SetRequest
        {
            ContentType = request.ContentType ?? String.Empty,
            Etag = ToEtag(request.ETag),
            Key = request.Key,
            Options = ToStateOptions(request.Options),
            Value = ByteString.CopyFrom(request.Value.ToArray())
        };

        grpcRequest.Metadata.Add(request.Metadata);

        return grpcRequest;
    }
}