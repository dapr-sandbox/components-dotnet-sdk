namespace Dapr.PluggableComponents.Components.StateStore;

public enum StateStoreTransactOperationType
{
    Delete,
    Set
}

public record class StateStoreTransactOperation(StateStoreTransactOperationType Type);
