using System;
using System.Text;
using Dapr.Client;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;

namespace ProxyComponentsSample.Components;

internal sealed class ProxyStateStore : IStateStore
{
    private readonly DaprClient daprClient;
    private readonly string? instanceId;
    private readonly ILogger<ProxyStateStore> logger;

    private string? storeName;

    public ProxyStateStore(
        string? instanceId,
        DaprClient daprClient,
        ILogger<ProxyStateStore> logger)
    {
        this.daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        this.instanceId = instanceId;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IStateStore Members

    public async Task BulkDeleteAsync(StateStoreBulkDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        await this.daprClient.DeleteBulkStateAsync(
            this.storeName,
            request
                .Items
                .Select(
                    item =>
                    {
                        return new BulkDeleteStateItem(ToProxyKey(item.Key), item.ETag, stateOptions: default, metadata: item.Metadata);
                    })
                .ToList(),
            cancellationToken);
    }

    public async Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        var responses = await this.daprClient.GetBulkStateAsync(
            this.storeName,
            request.Items.Select(item => ToProxyKey(item.Key)).ToList(),
            parallelism: null, // TODO: Where does parallelism come from?
            metadata: null,     // TODO: Individual requests have metadata, but not bulk request as a whole.
            cancellationToken: cancellationToken
        );

        var items = new List<StateStoreBulkStateItem>();

        // NOTE: Bulk APIs were added post-1.9.

        foreach (var response in responses)
        {
            StateStoreBulkStateItem item = null;

            if (response.Value != null)
            {
                item = new StateStoreBulkStateItem
                {
                    Data = Encoding.UTF8.GetBytes(response.Value)
                };
            }

            items.Add(item);
        }

        return new StateStoreBulkGetResponse
        {
            Got = items.Any(item => item.Data != null),
            Items = items
        };
    }

    public async Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        // NOTE: Bulk set API was added post-1.9.

        await Task.WhenAll(request.Items.Select(item => this.SetAsync(item, cancellationToken)));
    }

    public async Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Delete request for key {key}", request.Key);

        await this.daprClient.DeleteStateAsync(
            this.storeName,
            ToProxyKey(request.Key),
            stateOptions: default,
            metadata: request.Metadata,
            cancellationToken: cancellationToken);
    }

    public async Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("MemStateStore: Get request for key {key}", request.Key);

        // NOTE: Assume string?
        var value = await this.daprClient.GetStateAsync<string>(
            this.storeName,
            ToProxyKey(request.Key),
            consistencyMode: default,
            metadata: request.Metadata,
            cancellationToken: cancellationToken);

        StateStoreGetResponse response = null;

        if (value != null)
        {
            response = new StateStoreGetResponse
            {
                Data = Encoding.UTF8.GetBytes(value)
            };
        }

        return response;
    }

    public Task InitAsync(InitRequest request, CancellationToken cancellationToken = default)
    {
        if (request?.Metadata?.Properties?.TryGetValue("store-name", out var storeName) != true)
        {
            throw new InvalidOperationException("The component initialization metadata does not contain the store name.");
        }

        this.storeName = storeName;

        return Task.CompletedTask;
    }

    public async Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Set request for key {key}", request.Key);

        await this.daprClient.SaveStateAsync(
            this.storeName,
            ToProxyKey(request.Key),
            Encoding.UTF8.GetString(request.Value.Span),
            stateOptions: default,
            metadata: request.Metadata,
            cancellationToken: cancellationToken);
    }

    #endregion

    private static string ToProxyKey(string key)
    {
        return key.Replace("||", "--");
    }
}
