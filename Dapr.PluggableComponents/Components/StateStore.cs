namespace Dapr.PluggableComponents.Components;

public struct StoreObject
{
    public Byte[] Data { get; init; }
    public int ETag { get; init; }
}

public interface IStateStore
{
    Task<StoreObject?> Get(string key);
    void Init(Dictionary<string, string> properties);
    List<string> Features();
    Task Delete(string key, int etag);
    Task Set(string requestKey, StoreObject storeObject);
}

public enum Consistency
{
    Strong,
    Eventual,
    Unespecified,
}

public enum Concurrency
{
    FirstWrite,
    LastWrite,
    Unespecified,
}
public class Metadata : Dictionary<string, string> { }
public class StateStoreMetadata : Metadata { }
public struct GetStateOption
{
    public Consistency Consistency { get; init; }
}
public struct GetRequest
{
    public string Key { get; init; }
    public Metadata Metadata { get; init; }
    public GetStateOption Options { get; init; }
}

public struct GetResponse
{
    public byte[] Data { get; init; }
    public string? ETag { get; init; }
    public Metadata Metadata { get; init; }
    public string? ContentType { get; init; }
}

public struct DeleteStateOptions
{
    public Consistency Consistency { get; init; }
    public Concurrency Concurrency { get; init; }
}

public struct DeleteRequest
{
    public string Key { get; init; }
    public string? ETag { get; init; }
    public Metadata Metadata { get; init; }
    public DeleteStateOptions Options { get; init; }
}


public enum StateStoreFeature
{
    ETag,
    Transacional,
    Queriable,
}

public interface IStateStoreV2
{
    Task<GetResponse> Get(GetRequest req);
    void Init(StateStoreMetadata metadata);
    List<StateStoreFeature> Features();
    Task Delete(DeleteRequest req);
    Task Set(string requestKey, StoreObject storeObject);
}


public interface ITransactionalStateStore
{
    Task Transact();
}

public interface IQueriableStateStore
{
    Task Query();
}
