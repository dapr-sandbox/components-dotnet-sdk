using Dapr.PluggableComponents.Adaptors;
using Grpc.Core;
using Microsoft.AspNetCore.Connections.Features;

namespace Dapr.PluggableComponents;

internal sealed class RegisteredComponentProvider<TComponent> : IDaprPluggableComponentProvider<TComponent>
{
    private readonly DaprPluggableComponentsRegistry registry;
    private readonly IServiceProvider serviceProvider;

    public RegisteredComponentProvider(DaprPluggableComponentsRegistry registry, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.registry = registry;
        this.serviceProvider = serviceProvider;
    }

    public TComponent GetComponent(ServerCallContext context)
    {
        string socketPath = GetSocketPath(context);

        var componentProvider = this.registry.GetComponentProvider<TComponent>(this.serviceProvider, socketPath);

        if (componentProvider == null)
        {
            throw new InvalidOperationException($"Unable to obtain a provider for component type {typeof(TComponent)}.");
        }

        return componentProvider.GetComponent(context);
    }

    private static string GetSocketPath(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var socketFeature = httpContext.Features.Get<IConnectionSocketFeature>();        
        var socketPath = socketFeature?.Socket.LocalEndPoint?.ToString();

        if (socketPath == null)
        {
            throw new InvalidOperationException("Unable to determine the socket on which a gRPC request was made.");
        }

        return socketPath;
    }
}