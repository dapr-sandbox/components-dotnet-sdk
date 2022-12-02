namespace Dapr.PluggableComponents.Components.StateStore;

public enum TransactionalStateStoreTransactOperationType
{
    Delete,
    Set
}

public abstract class TransactionalStateStoreTransactOperation
{
    protected TransactionalStateStoreTransactOperation(TransactionalStateStoreTransactOperationType type)
    {
        this.Type = type;
    }

    public TransactionalStateStoreTransactOperationType Type { get; }
}

public sealed class TransactionalStateStoreTransactDeleteOperation : TransactionalStateStoreTransactOperation
{
    public TransactionalStateStoreTransactDeleteOperation(StateStoreDeleteRequest request)
        : base(TransactionalStateStoreTransactOperationType.Delete)
    {
        this.Request = request;
    }

    public StateStoreDeleteRequest Request { get; }
}

public sealed class TransactionalStateStoreTransactSetOperation : TransactionalStateStoreTransactOperation
{
    public TransactionalStateStoreTransactSetOperation(StateStoreSetRequest request)
        : base(TransactionalStateStoreTransactOperationType.Set)
    {
        this.Request = request;
    }

    public StateStoreSetRequest Request { get; }
}

public sealed class TransactionalStateStoreTransactRequest
{
    public TransactionalStateStoreTransactOperation[] Operations { get; init; } = Array.Empty<TransactionalStateStoreTransactOperation>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}