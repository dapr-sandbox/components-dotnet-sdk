using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Proxies.Grpc.v1;
using Google.Protobuf;
using Grpc.Net.Client;

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

    public Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        return Task.CompletedTask;
    }

    public Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        return Task.FromResult(new StateStoreBulkGetResponse(false)
        {
        });
    }

    public async Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new BulkSetRequest
        {
        };

        grpcRequest.Items.Add(
            request
                .Items
                .Select(
                    item => new SetRequest
                    {
                        ContentType = item.ContentType ?? String.Empty,
                        Etag = item.ETag != null ? new Etag { Value = item.ETag } : null,
                        Key = item.Key,
                        // Metadata
                        // Options
                        Value = ByteString.CopyFrom(item.Value.ToArray())
                    }));

        await client.BulkSetAsync(
            grpcRequest,
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Delete request for key {key}", request.Key);

        return Task.CompletedTask;
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("MemStateStore: Get request for key {key}", request.Key);

        return Task.FromResult<StateStoreGetResponse?>(null);
    }

    public async Task InitAsync(Dapr.PluggableComponents.Components.InitRequest request, CancellationToken cancellationToken = default)
    {
        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new Dapr.PluggableComponents.Proxies.Grpc.v1.InitRequest
        {
            Metadata = new Dapr.PluggableComponents.Proxies.Grpc.v1.MetadataRequest
            {
            }
        };

        foreach (var kvp in request.Metadata?.Properties ?? Enumerable.Empty<KeyValuePair<string, string>>())
        {
            grpcRequest.Metadata.Properties.Add(kvp.Key, kvp.Value);
        }

        await client.InitAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Set request for key {key}", request.Key);

        var client = new StateStore.StateStoreClient(this.grpcChannelProvider.GetChannel());

        await client.SetAsync(
            new SetRequest
            {
                ContentType = request.ContentType ?? String.Empty,
                Etag = request.ETag != null ? new Etag { Value = request.ETag } : null,
                Key = request.Key,
                // Metadata
                // Options
                Value = ByteString.CopyFrom(request.Value.ToArray())
            },
            cancellationToken: cancellationToken);
    }

    #endregion
}