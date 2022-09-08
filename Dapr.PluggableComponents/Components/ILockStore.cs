namespace Dapr.PluggableComponents.Components;

public struct LockResult
{
    public bool Locked;
    public bool Success;
}

public interface ILockStore
{

    void Init(Dictionary<string,string> properties);
    LockResult TryLock(string requestLockOwner, string requestResourceId, int requestExpiryInSeconds);
    LockResult Unlock(string requestLockOwner, string requestResourceId);
}
