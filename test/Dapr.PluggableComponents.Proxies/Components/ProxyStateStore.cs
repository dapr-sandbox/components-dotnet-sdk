using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Proxies.Grpc.v1;
using Dapr.PluggableComponents.Proxies.Utilities;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Proxies.Components;

internal sealed class ProxyStateStore : IStateStore
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

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new BulkDeleteRequest();

        grpcRequest.Items.AddRange(
            request.Items.Select(ToDeleteRequest));

        await client.BulkDeleteAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new BulkGetRequest();

        grpcRequest.Items.Add(request.Items.Select(ToGetRequest));

        var grpcResponse = await client.BulkGetAsync(
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
        this.logger?.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new BulkSetRequest();

        grpcRequest.Items.Add(
            request
                .Items
                .Select(ToSetRequest));

        await client.BulkSetAsync(
            grpcRequest,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Delete request for key {key}", request.Key);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        await client.DeleteAsync(
            ToDeleteRequest(request),
            cancellationToken: cancellationToken);
    }

    public async Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Get request for key {key}", request.Key);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var response = await client.GetAsync(
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

    public async Task InitAsync(Dapr.PluggableComponents.Components.InitRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Init request");

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new Dapr.PluggableComponents.Proxies.Grpc.v1.InitRequest
        {
            Metadata = new Dapr.PluggableComponents.Proxies.Grpc.v1.MetadataRequest()
        };

        grpcRequest.Metadata.Properties.Add(request.Metadata?.Properties ?? Enumerable.Empty<KeyValuePair<string, string>>());

        await client.InitAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Set request for key {key}", request.Key);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        await client.SetAsync(
            ToSetRequest(request),
            cancellationToken: cancellationToken);
    }

    #endregion

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