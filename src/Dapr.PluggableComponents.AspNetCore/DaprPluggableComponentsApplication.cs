using System.Collections.Concurrent;
using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.Components.StateStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplication
{
    public static DaprPluggableComponentsApplication Create()
    {
        return Create(new DaprPluggableComponentsApplicationOptions());
    }

    public static DaprPluggableComponentsApplication Create(string socketName)
    {
        return Create(new DaprPluggableComponentsApplicationOptions { SocketName = socketName });
    }

    public static DaprPluggableComponentsApplication Create(DaprPluggableComponentsApplicationOptions options)
    {
        return new DaprPluggableComponentsApplication(options);
    }

    private readonly DaprPluggableComponentsApplicationOptions options;

    private readonly ConcurrentBag<Action<WebApplicationBuilder>> builderActions = new ConcurrentBag<Action<WebApplicationBuilder>>();
    private readonly ConcurrentBag<Action<WebApplication>> appActions = new ConcurrentBag<Action<WebApplication>>();

    private DaprPluggableComponentsApplication(DaprPluggableComponentsApplicationOptions options)
    {       
        this.options = options;
    }

    #region State Store Members

    public void AddStateStore<TStateStore>(Func<IServiceProvider, string?, TStateStore> stateStoreFactory)
        where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>(stateStoreFactory);

        this.AddRelatedStateStoreServices<TStateStore>();
    }

    public void AddStateStore<TStateStore>() where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>();

        this.AddRelatedStateStoreServices<TStateStore>();
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

    #endregion

    public void Run()
    {
        this.CreateApplication().Run();        
    }

    public Task RunAsync()
    {
        return this.CreateApplication().RunAsync();
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>(Func<IServiceProvider, string?, TComponentImpl> pubSubFactory)
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponentType>>(serviceProvider => new MultiplexedComponentProvider<TComponentType>(serviceProvider, pubSubFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();                
            });
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>()
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<TComponentType, TComponentImpl>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponentType>, SingletonComponentProvider<TComponentType>>();
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();
            });
    }

    private void AddRelatedService<TComponent, TComponentImpl, TAdaptor>()
        where TComponent : class
        where TAdaptor : class
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponent>, DelegatedComponentProvider<TComponent, TComponentImpl>>();
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();
            });
    }

    private void ConfigureApplication(Action<WebApplication> configurer)
    {
        this.appActions.Add(configurer);
    }

    private void ConfigureApplicationBuilder(Action<WebApplicationBuilder> configurer)
    {
        this.builderActions.Add(configurer);
    }

    private WebApplication CreateApplication()
    {
        var builder = this.options.WebApplicationOptions != null
            ? WebApplication.CreateBuilder(this.options.WebApplicationOptions)
            : WebApplication.CreateBuilder();

        this.options.WebApplicationBuilderConfiguration?.Invoke(builder);

        builder.AddDaprPluggableComponentsServices(options);

        foreach (var configurer in this.builderActions)
        {
            configurer(builder);
        }

        var app = builder.Build();

        this.options.WebApplicationConfiguration?.Invoke(app);

        app.MapDaprPluggableComponentsServices();

        foreach (var configurer in this.appActions)
        {
            configurer(app);
        }

        return app;
    }
}