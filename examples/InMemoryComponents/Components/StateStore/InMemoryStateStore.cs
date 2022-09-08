using Dapr.PluggableComponents.Components;

namespace DaprInMemoryComponents.Components.StateStore;

public class InMemoryStateStore : IStateStore
{
    private Dictionary<String, StoreObject> dataStore;

    public InMemoryStateStore()
    {
        dataStore = new Dictionary<string, StoreObject>();
    }

    public Task<StoreObject?> Get(string requestKey)
    {
        if (dataStore.ContainsKey(requestKey))
        {
            return Task.FromResult<StoreObject?>(dataStore[requestKey]);
        }
        else
        {
            return Task.FromResult<StoreObject?>(null);
        }
    }

    public void Init(Dictionary<string, string> props)
    {
    }

    public List<string> Features()
    {
        return new List<string>();
    }

    public Task Delete(string requestKey, int etag)
    {
        return Task.FromResult(dataStore.Remove(requestKey));
    }

    public Task Set(string requestKey, StoreObject storeObject)
    {
        return Task.FromResult(dataStore[requestKey] = storeObject);
    }
}
