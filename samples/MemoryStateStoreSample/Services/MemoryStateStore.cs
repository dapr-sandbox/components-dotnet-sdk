using System.Collections.Concurrent;
using System.Text;

namespace MemoryStateStoreSample.Services;

internal sealed class MemoryStateStore : IStateStore
{
    private readonly ILogger<MemoryStateStore> logger;

    private readonly IDictionary<string, string?> storage = new ConcurrentDictionary<string, string?>();

    public MemoryStateStore(ILogger<MemoryStateStore> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MemStateStore: Get request for key {key}", request.Key);

        if (this.storage.TryGetValue(request.Key, out var data))
        {
            return Task.FromResult<StateStoreGetResponse?>(new StateStoreGetResponse
            {
                Data = Encoding.UTF8.GetBytes(data!),
            });
        }

        return Task.FromResult<StateStoreGetResponse?>(null);
    }

    public Task InitAsync(StateStoreInitMetadata metadata, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MemStore: Set request for key {key}", request.Key);

        this.storage[request.Key] = Encoding.UTF8.GetString(request.Value.Span);

        return Task.CompletedTask;
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
}