using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.Components.StateStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsServiceBuilder
{
    private string socketPath;
    private Action<Action<WebApplicationBuilder>> configureApplicationBuilder;
    private Action<Action<WebApplication>> configureApplication;

    internal DaprPluggableComponentsServiceBuilder(
        string socketPath,
        Action<Action<WebApplicationBuilder>> configureApplicationBuilder,
        Action<Action<WebApplication>> configureApplication)
    {
        this.socketPath = socketPath;
        this.configureApplicationBuilder = configureApplicationBuilder;
        this.configureApplication = configureApplication;
    }

    public DaprPluggableComponentsServiceBuilder RegisterStateStore<TStateStore>() where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>();

        this.AddRelatedStateStoreServices<TStateStore>();

        return this;
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>()
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.configureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<TComponentType, TComponentImpl>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponentType>, SingletonComponentProvider<TComponentType>>();
            });

        this.configureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();
            });
    }

    private void AddRelatedStateStoreServices<TStateStore>()
    {
        if (typeof(TStateStore).IsAssignableTo(typeof(IQueryableStateStore)))
        {
            this.AddRelatedService<IQueryableStateStore, IStateStore, QueryableStateStoreAdaptor>();
        }

        if (typeof(TStateStore).IsAssignableTo(typeof(ITransactionalStateStore)))
        {
            this.AddRelatedService<ITransactionalStateStore, IStateStore, TransactionalStateStoreAdaptor>();
        }
    }

    private void AddRelatedService<TComponent, TComponentImpl, TAdaptor>()
        where TComponent : class
        where TAdaptor : class
    {
        this.configureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponent>, DelegatedComponentProvider<TComponent, TComponentImpl>>();
            });

        this.configureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();
            });
    }
}