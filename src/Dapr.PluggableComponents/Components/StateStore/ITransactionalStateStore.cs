namespace Dapr.PluggableComponents.Components.StateStore;

public interface ITransactionalStateStore
{
    Task TransactAsync(StateStoreTransactRequest request, CancellationToken cancellationToken = default);
}
