using System.Collections.Concurrent;
using Dapr.PluggableComponents.Adaptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Mono.Unix;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplication : IDaprPluggableComponentsRegistrar
{
    public static DaprPluggableComponentsApplication Create()
    {
        return Create(new DaprPluggableComponentsApplicationOptions());
    }

    public static DaprPluggableComponentsApplication Create(DaprPluggableComponentsApplicationOptions options)
    {
        return new DaprPluggableComponentsApplication(options);
    }

    private readonly DaprPluggableComponentsApplicationOptions options;

    private readonly ConcurrentBag<Action<WebApplicationBuilder>> builderActions = new ConcurrentBag<Action<WebApplicationBuilder>>();
    private readonly ConcurrentBag<Action<WebApplication>> appActions = new ConcurrentBag<Action<WebApplication>>();

    private readonly ConcurrentBag<DaprServiceRegistration> serviceBuilderActions = new ConcurrentBag<DaprServiceRegistration>();

    private readonly ConcurrentDictionary<Type, bool> registeredAdaptors = new ConcurrentDictionary<Type, bool>();
    private readonly ConcurrentDictionary<Type, bool> registeredComponents = new ConcurrentDictionary<Type, bool>();
    private readonly ConcurrentDictionary<Type, bool> registeredProviders = new ConcurrentDictionary<Type, bool>();

    private DaprPluggableComponentsApplication(DaprPluggableComponentsApplicationOptions options)
    {       
        this.options = options;
    }

    private sealed record DaprServiceRegistration(DaprPluggableComponentsServiceOptions Options, Action<DaprPluggableComponentsServiceBuilder> Callback);

    public DaprPluggableComponentsApplication RegisterService(string socketName, Action<DaprPluggableComponentsServiceBuilder> callback)
    {
        return this.RegisterService(new DaprPluggableComponentsServiceOptions(socketName), callback);
    }

    public DaprPluggableComponentsApplication RegisterService(DaprPluggableComponentsServiceOptions options, Action<DaprPluggableComponentsServiceBuilder> callback)
    {
        this.serviceBuilderActions.Add(new DaprServiceRegistration(options, callback));

        return this;
    }

    public void Run()
    {
        this.CreateApplication().Run();        
    }

    public Task RunAsync()
    {
        return this.CreateApplication().RunAsync();
    }

    #region IDaprPluggableComponentsRegistrar Members

    void IDaprPluggableComponentsRegistrar.RegisterAdaptor<TAdaptor>()
        where TAdaptor : class
    {
        if (this.registeredAdaptors.TryAdd(typeof(TAdaptor), true))
        {
            this.ConfigureApplication(
                app =>
                {
                    app.MapDaprPluggableComponentAdaptor<TAdaptor>();
                });
        }
    }

    void IDaprPluggableComponentsRegistrar.RegisterComponent<TComponent>(string socketPath) where TComponent : class
    {
        if (this.registeredComponents.TryAdd(typeof(TComponent), true))
        {
            this.ConfigureApplicationBuilder(
                builder =>
                {
                    builder.Services.AddSingleton<TComponent, TComponent>();
               });
        }

        ((IDaprPluggableComponentsRegistrar)this).RegisterComponent(socketPath, context => context.ServiceProvider.GetRequiredService<TComponent>());
    }

    private readonly DaprPluggableComponentsRegistry registry = new DaprPluggableComponentsRegistry();    

    void IDaprPluggableComponentsRegistrar.RegisterComponent<TComponent>(string socketPath, ComponentProviderDelegate<TComponent> componentFactory) where TComponent : class
    {
        this.registry.RegisterComponentProvider(socketPath, serviceProvider => new MultiplexedComponentProvider<TComponent>(componentFactory, serviceProvider, socketPath));
    }

    void IDaprPluggableComponentsRegistrar.RegisterProvider<TComponent, TComponentImpl>(string socketPath)
        where TComponent : class
        where TComponentImpl : class
    {
        if (this.registeredProviders.TryAdd(typeof(TComponent), true))
        {
            this.ConfigureApplicationBuilder(
                builder =>
                {
                    builder.Services.AddSingleton<IDaprPluggableComponentProvider<TComponent>>(serviceProvider => new RegisteredComponentProvider<TComponent>(this.registry, serviceProvider));
                });
        }

        this.registry.RegisterComponentProvider<TComponent, TComponentImpl>(socketPath);
    }

    #endregion

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

        builder.AddDaprPluggableComponentsSupportServices();

        var socketPaths = new HashSet<string>();

        foreach (var registration in this.serviceBuilderActions)
        {
            string socketPath = builder.AddDaprService(registration.Options);
            
            socketPaths.Add(socketPath);

            var serviceBuilder = new DaprPluggableComponentsServiceBuilder(socketPath, this);

            registration.Callback(serviceBuilder);
        }

        foreach (var configurer in this.builderActions)
        {
            configurer(builder);
        }

        var app = builder.Build();

        this.options.WebApplicationConfiguration?.Invoke(app);

        app.MapDaprPluggableComponentsSupportServices();

        foreach (var configurer in this.appActions)
        {
            configurer(app);
        }

        app.Lifetime.ApplicationStarted.Register(
            () =>
            {
                // NOTE:
                //
                // In Kubernetes, the creator of the socket file (this pluggable component) will not be the same user
                // as the reader/writer of the socket file (the Dapr sidecar), unlike when running the component
                // locally. Therefore, once the socket file has been created (after start), the permissions need be
                // updated to allow global read/write.

                foreach (var socketPath in socketPaths)
                {
                    var fileInfo = new UnixFileInfo(socketPath);

                    fileInfo.FileAccessPermissions =
                        FileAccessPermissions.UserRead | FileAccessPermissions.UserWrite
                        | FileAccessPermissions.GroupRead | FileAccessPermissions.GroupWrite
                        | FileAccessPermissions.OtherRead | FileAccessPermissions.OtherWrite;
                }
            });

        return app;
    }
}