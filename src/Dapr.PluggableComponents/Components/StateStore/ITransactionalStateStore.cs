namespace Dapr.PluggableComponents.Components.StateStore;

public interface ITransactionalStateStore
{
    Task TransactAsync(TransactionalStateStoreTransactRequest request, CancellationToken cancellationToken = default);
}
