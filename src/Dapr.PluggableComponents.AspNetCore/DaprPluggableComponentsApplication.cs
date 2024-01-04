// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System.Collections.Concurrent;
using Dapr.PluggableComponents.Adaptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mono.Unix;

namespace Dapr.PluggableComponents;

/// <summary>
/// Represents an application that hosts Dapr Pluggable Components.
/// </summary>
public sealed class DaprPluggableComponentsApplication : IDaprPluggableComponentsRegistrar, IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Creates a <see cref="DaprPluggableComponentsApplication"/> instance.
    /// </summary>
    /// <returns></returns>
    public static DaprPluggableComponentsApplication Create()
    {
        return new DaprPluggableComponentsApplication(WebApplication.CreateBuilder());
    }

    private readonly ConcurrentBag<Action<WebApplicationBuilder>> builderActions = new ConcurrentBag<Action<WebApplicationBuilder>>();
    private readonly ConcurrentBag<Action<WebApplication>> appActions = new ConcurrentBag<Action<WebApplication>>();
    private readonly ConcurrentDictionary<Type, bool> registeredAdaptors = new ConcurrentDictionary<Type, bool>();
    private readonly ConcurrentDictionary<Type, bool> registeredComponents = new ConcurrentDictionary<Type, bool>();
    private readonly ConcurrentDictionary<Type, bool> registeredProviders = new ConcurrentDictionary<Type, bool>();
    private readonly DaprPluggableComponentsRegistry registry = new DaprPluggableComponentsRegistry();
    private readonly ConcurrentBag<DaprServiceRegistration> serviceBuilderActions = new ConcurrentBag<DaprServiceRegistration>();
    private readonly WebApplicationBuilder webApplicationBuilder;
    private readonly Lazy<WebApplication> webApplicationProvider;

    private DaprPluggableComponentsApplication(WebApplicationBuilder webApplicationBuilder)
    {
        this.webApplicationBuilder = webApplicationBuilder ?? throw new ArgumentNullException(nameof(webApplicationBuilder));
        this.webApplicationProvider = new Lazy<WebApplication>(this.CreateApplication);
    }

    private sealed record DaprServiceRegistration(DaprPluggableComponentsServiceOptions Options, Action<DaprPluggableComponentsServiceBuilder> Callback);

    /// <summary>
    /// Gets a collection of configuration providers hosted by the application.
    /// </summary>
    public ConfigurationManager Configuration => this.webApplicationBuilder.Configuration;

    /// <summary>
    /// Gets a collection of logging providers hosted by the application.
    /// </summary>
    public ILoggingBuilder Logging => this.webApplicationBuilder.Logging;

    /// <summary>
    /// Gets a collection of services hosted by the application.
    /// </summary>
    public IServiceCollection Services => this.webApplicationBuilder.Services;

    /// <summary>
    /// Registers a "service" (i.e. a set of Dapr Pluggable Components) exposed via a specific socket.
    /// </summary>
    /// <param name="socketName">The name of the socket (without extension).</param>
    /// <param name="callback">A callback from which to register components with the service.</param>
    /// <returns>The current <see cref="DaprPluggableComponentsApplication"/> instance.</returns>
    public DaprPluggableComponentsApplication RegisterService(string socketName, Action<DaprPluggableComponentsServiceBuilder> callback)
    {
        return this.RegisterService(new DaprPluggableComponentsServiceOptions(socketName), callback);
    }

    /// <summary>
    /// Registers a "service" (i.e. a set of Dapr Pluggable Components) exposed via a specific socket.
    /// </summary>
    /// <param name="options">Options related to the creation of the socket file.</param>
    /// <param name="callback">A callback from which to register components with the service.</param>
    /// <returns>The current <see cref="DaprPluggableComponentsApplication"/> instance.</returns>
    public DaprPluggableComponentsApplication RegisterService(DaprPluggableComponentsServiceOptions options, Action<DaprPluggableComponentsServiceBuilder> callback)
    {
        this.serviceBuilderActions.Add(new DaprServiceRegistration(options, callback));

        return this;
    }

    /// <summary>
    /// Runs the application and blocks the calling thread until shutdown.
    /// </summary>
    public void Run()
    {
        this.webApplicationProvider.Value.Run();
    }

    /// <summary>
    /// Runs the application asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when shutdown.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var application = this.webApplicationProvider.Value;

        var listener = cancellationToken.Register(
            () =>
            {
                application.Lifetime.StopApplication();
            });

        try
        {
            // NOTE: RunAsync() doesn't accept a cancellation token (despite the docs); that's slated for .NET 8.
            //       See: https://github.com/dotnet/aspnetcore/issues/44083
            await application.RunAsync();
        }
        finally
        {
            listener.Dispose();
        }
    }

    /// <summary>
    /// Start the application.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the startup of the application. Successfull completion indicates the server is ready to accept new requests.</returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return this.webApplicationProvider.Value.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the shutdown of the application. Successfull completion indicates the server has stopped.</returns>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return this.webApplicationProvider.Value.StopAsync(cancellationToken);
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

    #region IAsyncDisposable Members

    /// <summary>
    /// Disposes the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the application is disposed.</returns>
    public ValueTask DisposeAsync()
    {
        if (this.webApplicationProvider.IsValueCreated)
        {
            return this.webApplicationProvider.Value.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region IDisposable Members

    void IDisposable.Dispose()
    {
        if (this.webApplicationProvider.IsValueCreated && this.webApplicationProvider.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
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
        this.webApplicationBuilder.AddDaprPluggableComponentsSupportServices();

        var socketPaths = new HashSet<string>();

        foreach (var registration in this.serviceBuilderActions)
        {
            string socketPath = this.webApplicationBuilder.AddDaprService(registration.Options);

            socketPaths.Add(socketPath);

            var serviceBuilder = new DaprPluggableComponentsServiceBuilder(socketPath, this);

            registration.Callback(serviceBuilder);
        }

        foreach (var configurer in this.builderActions)
        {
            configurer(this.webApplicationBuilder);
        }

        var app = this.webApplicationBuilder.Build();

        app.MapDaprPluggableComponentsSupportServices();

        foreach (var configurer in this.appActions)
        {
            configurer(app);
        }

        if (!OperatingSystem.IsWindows())
        {
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
        }

        return app;
    }
}
