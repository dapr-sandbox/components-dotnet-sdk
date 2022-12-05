using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public enum StateStoreConcurrency
{
    Unspecified = 0,
    FirstWrite = 1,
    LastWrite = 2
}

public enum StateStoreConsistency
{
    Unspecified = 0,
    Eventual = 1,
    Strong = 2
}

public sealed record StateStoreStateOptions
{
    public StateStoreConcurrency Concurrency { get; init; } = StateStoreConcurrency.Unspecified;

    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    public static StateStoreStateOptions? FromStateOptions(StateOptions options)
    {
        return options != null
            ? new StateStoreStateOptions
                {
                    Concurrency = (StateStoreConcurrency)options.Concurrency,
                    Consistency = (StateStoreConsistency)options.Consistency
                }
            : null;
    }
}
