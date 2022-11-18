using System.Collections.Concurrent;
using System.Text;

namespace MemoryStateStoreSample.Services;

internal sealed class MemoryStateStore : IStateStore
{
    private readonly ILogger<MemoryStateStore> logger;

    private readonly IDictionary<string, string> storage = new ConcurrentDictionary<string, string>();

    public MemoryStateStore(ILogger<MemoryStateStore> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<StateStoreBulkGetResponse> BulkGetAsync(StateStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkGet request for {count} keys", request.Items.Count);

        var items = new List<StateStoreBulkStateItem>();

        foreach (var itemRequest in request.Items)
        {
            var item = new StateStoreBulkStateItem();

            if (this.storage.TryGetValue(itemRequest.Key, out var value))
            {
                item = new StateStoreBulkStateItem
                {
                    Data = Encoding.UTF8.GetBytes(value)
                };
            }

            items.Add(item);
        }

        return Task.FromResult(
            new StateStoreBulkGetResponse
            {
                Got = items.Any(item => item.Data != null),
                Items = items
            });
    }

    public Task BulkSetAsync(StateStoreBulkSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("BulkSet request for {count} keys", request.Items.Count);

        foreach (var item in request.Items)
        {
            this.storage[item.Key] = Encoding.UTF8.GetString(item.Value.Span);
        }

        return Task.CompletedTask;
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MemStateStore: Get request for key {key}", request.Key);

        StateStoreGetResponse? response = null;

        if (this.storage.TryGetValue(request.Key, out var data))
        {
            response = new StateStoreGetResponse
            {
                Data = Encoding.UTF8.GetBytes(data)
            };
        }

        return Task.FromResult(response);
    }

    public Task InitAsync(StateStoreInitRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MemStore: Set request for key {key}", request.Key);

        this.storage[request.Key] = Encoding.UTF8.GetString(request.Value.Span);

        return Task.CompletedTask;
    }
}