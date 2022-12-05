namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreTransactDeleteOperation(StateStoreDeleteRequest Request)
    : StateStoreTransactOperation(StateStoreTransactOperationType.Delete);
