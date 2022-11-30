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

    public void UseInputBinding(Func<string?, IInputBinding> pubSubFactory)
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IInputBinding>>(_ => new MultiplexedComponentProvider<IInputBinding>(pubSubFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapInputBinding<InputBindingAdaptor>();                
            });
    }

    public void UseInputBinding<TInputBinding>() where TInputBinding : class, IInputBinding
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IInputBinding, TInputBinding>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IInputBinding>, SingletonComponentProvider<IInputBinding>>();
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapInputBinding<InputBindingAdaptor>();                
            });
    }

    #endregion

    #region Output Binding Members

    public void UseOutputBinding(Func<string?, IOutputBinding> pubSubFactory)
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IOutputBinding>>(_ => new MultiplexedComponentProvider<IOutputBinding>(pubSubFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapOutputBinding<OutputBindingAdaptor>();                
            });
    }

    public void UseOutputBinding<TOutputBinding>() where TOutputBinding : class, IOutputBinding
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IOutputBinding, TOutputBinding>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IOutputBinding>, SingletonComponentProvider<IOutputBinding>>();
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapOutputBinding<OutputBindingAdaptor>();                
            });
    }

    #endregion

    #region PubSub Members

    public void UsePubSub(Func<string?, IPubSub> pubSubFactory)
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IPubSub>>(_ => new MultiplexedComponentProvider<IPubSub>(pubSubFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapPubSub<PubSubAdaptor>();                
            });
    }

    public void UsePubSub<TPubSub>() where TPubSub : class, IPubSub
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IPubSub, TPubSub>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IPubSub>, SingletonComponentProvider<IPubSub>>();
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapPubSub<PubSubAdaptor>();                
            });
    }

    #endregion

    #region State Store Members

    public void UseStateStore(Func<string?, IStateStore> stateStoreFactory)
    {
        this.ConfigureApplicationBuilder(
            builder =>
            {
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IStateStore>>(_ => new MultiplexedComponentProvider<IStateStore>(stateStoreFactory));
            });

        this.ConfigureApplication(
            app =>
            {
                app.MapStateStore<StateStoreAdaptor>();                
            });
    }

    public void UseStateStore<TStateStore>() where TStateStore : class, IStateStore
    {
        this.ConfigureApplicationBuilder(
            (Action<WebApplicationBuilder>)(            builder =>
            {
                builder.Services.AddSingleton<IStateStore, TStateStore>();
                builder.Services.AddSingleton<IDaprPluggableComponentProvider<IStateStore>, SingletonComponentProvider<IStateStore>>();
            }));

        this.ConfigureApplication(
            app =>
            {
                app.MapStateStore<StateStoreAdaptor>();                
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

        builder.UseDaprPluggableComponents(options);

        foreach (var configurer in this.builderActions)
        {
            configurer(builder);
        }

        var app = builder.Build();

        this.options.WebApplicationConfiguration?.Invoke(app);

        app.MapDaprPluggableComponents();

        foreach (var configurer in this.appActions)
        {
            configurer(app);
        }

        return app;
    }
}