namespace Dapr.PluggableComponents.Components;

public struct StoreObject
{
    public Byte[] data { get; init; }
    public int etag { get; init; }
}

public interface IStateStore
{
    Task<StoreObject?> Get(string key);
    void Init(Dictionary<string,string> properties);
    List<string> Features();
    Task Delete(string key, int etag);
    Task Set(string requestKey, StoreObject storeObject);
}
