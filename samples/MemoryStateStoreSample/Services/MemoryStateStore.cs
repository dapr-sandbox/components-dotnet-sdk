using System.Collections.Concurrent;
using System.Text;
using Dapr.PluggableComponents.Components;

namespace MemoryStateStoreSample.Services;

internal sealed class MemoryStateStore : IStateStore
{
    private readonly ILogger<MemoryStateStore> logger;

    private readonly IDictionary<string, string> storage = new ConcurrentDictionary<string, string>();

    public MemoryStateStore(ILogger<MemoryStateStore> logger)
    {
        this.logger = logger;
    }

    #region IStateStore Members

    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Delete request for key {key}", request.Key);

        this.storage.Remove(request.Key);

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

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MemStore: Set request for key {key}", request.Key);

        this.storage[request.Key] = Encoding.UTF8.GetString(request.Value.Span);

        return Task.CompletedTask;
    }

    #endregion
}