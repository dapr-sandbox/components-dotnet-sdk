using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.Components.StateStore;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsServiceBuilder
{
    private readonly string socketPath;
    private readonly IDaprPluggableComponentsRegistrar registrar;

    internal DaprPluggableComponentsServiceBuilder(
        string socketPath,
        IDaprPluggableComponentsRegistrar registrar)
    {
        this.socketPath = socketPath;
        this.registrar = registrar;
    }

    public DaprPluggableComponentsServiceBuilder RegisterStateStore<TStateStore>() where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>();

        this.AddRelatedStateStoreServices<TStateStore>();

        return this;
    }

    public DaprPluggableComponentsServiceBuilder RegisterStateStore<TStateStore>(Func<IServiceProvider, string?, TStateStore> stateStoreFactory)
        where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>(stateStoreFactory);

        this.AddRelatedStateStoreServices<TStateStore>();
        
        return this;
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>()
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.registrar.RegisterComponent<TComponentImpl>(this.socketPath);

        this.AddRelatedService<TComponentType, TComponentImpl, TAdaptor>();
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>(Func<IServiceProvider, string?, TComponentImpl> pubSubFactory)
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.registrar.RegisterComponent<TComponentImpl>(socketPath, pubSubFactory);

        this.AddRelatedService<TComponentType, TComponentImpl, TAdaptor>();
    }

    private void AddRelatedStateStoreServices<TStateStore>()
        where TStateStore : class
    {
        if (typeof(TStateStore).IsAssignableTo(typeof(IQueryableStateStore)))
        {
            this.AddRelatedService<IQueryableStateStore, TStateStore, QueryableStateStoreAdaptor>();
        }

        if (typeof(TStateStore).IsAssignableTo(typeof(ITransactionalStateStore)))
        {
            this.AddRelatedService<ITransactionalStateStore, TStateStore, TransactionalStateStoreAdaptor>();
        }
    }

    private void AddRelatedService<TComponent, TComponentImpl, TAdaptor>()
        where TComponent : class
        where TComponentImpl : class
        where TAdaptor : class
    {
        this.registrar.RegisterProvider<TComponent, TComponentImpl>(this.socketPath);

        this.registrar.RegisterAdaptor<TAdaptor>();
    }
}