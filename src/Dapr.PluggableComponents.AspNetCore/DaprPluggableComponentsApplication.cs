using System.Collections.Concurrent;
using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.PluggableComponents.Components.StateStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplication
{
    public static DaprPluggableComponentsApplication Create()
    {
        return Create(new DaprPluggableComponentsOptions());
    }

    public static DaprPluggableComponentsApplication Create(string[] args)
    {
        return Create(new DaprPluggableComponentsOptions { Args = args });
    }

    public static DaprPluggableComponentsApplication Create(DaprPluggableComponentsOptions options)
    {
        return new DaprPluggableComponentsApplication(options);
    }

    private readonly DaprPluggableComponentsOptions options;

    private readonly ConcurrentBag<Action<WebApplicationBuilder>> builderActions = new ConcurrentBag<Action<WebApplicationBuilder>>();
    private readonly ConcurrentBag<Action<WebApplication>> appActions = new ConcurrentBag<Action<WebApplication>>();

    private DaprPluggableComponentsApplication(DaprPluggableComponentsOptions options)
    {       
        this.options = options;
    }

    #region Input Binding Members

    public void AddInputBinding(Func<string?, IInputBinding> pubSubFactory)
        => this.AddComponent<IInputBinding, InputBindingAdaptor>(pubSubFactory);

    public void AddInputBinding<TInputBinding>()
        where TInputBinding : class, IInputBinding
        => this.AddComponent<IInputBinding, TInputBinding, InputBindingAdaptor>();

    #endregion

    #region Output Binding Members

    public void AddOutputBinding(Func<string?, IOutputBinding> pubSubFactory)
        => this.AddComponent<IOutputBinding, OutputBindingAdaptor>(pubSubFactory);

    public void AddOutputBinding<TOutputBinding>()
        where TOutputBinding : class, IOutputBinding
        => this.AddComponent<IOutputBinding, TOutputBinding, OutputBindingAdaptor>();

    #endregion

    #region PubSub Members

    public void AddPubSub(Func<string?, IPubSub> pubSubFactory)
        => this.AddComponent<IPubSub, PubSubAdaptor>(pubSubFactory);

    public void AddPubSub<TPubSub>()
        where TPubSub : class, IPubSub
        => this.AddComponent<IPubSub, TPubSub, PubSubAdaptor>();

    #endregion

    #region State Store Members

    public void AddStateStore(Func<string?, IStateStore> stateStoreFactory)
        => this.AddComponent<IStateStore, StateStoreAdaptor>(stateStoreFactory);


    public void AddStateStore<TStateStore>()
        where TStateStore : class, IStateStore
        => this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>();

    #endregion

    #region Component Members

    public void AddComponent<TComponent, TAdaptor>(Func<string?, TComponent> pubSubFactory)
        where TAdaptor : class
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponent>>(_ => new MultiplexedComponentProvider<TComponent>(pubSubFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapDaprPluggableComponent<TAdaptor>();                
            });
    }

    public void AddComponent<TComponentType, TComponentImpl, TAdaptor>()
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

    #endregion

    public void Run()
    {
        this.CreateApplication().Run();        
    }

    public Task RunAsync()
    {
        return this.CreateApplication().RunAsync();
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
        var builder =
            this.options.Args != null
                ? WebApplication.CreateBuilder(this.options.Args)
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