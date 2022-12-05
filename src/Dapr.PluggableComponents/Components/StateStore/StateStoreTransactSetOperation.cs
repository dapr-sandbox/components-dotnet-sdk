namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreTransactSetOperation(StateStoreSetRequest Request)
    : StateStoreTransactOperation(StateStoreTransactOperationType.Set);
