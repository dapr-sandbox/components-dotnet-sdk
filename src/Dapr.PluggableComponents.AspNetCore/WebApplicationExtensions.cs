using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dapr.PluggableComponents;

public static class WebApplicationExtensions
{
    public static TBuilder MapDaprPluggableComponents<TBuilder>(this TBuilder app)
        where TBuilder : IEndpointRouteBuilder
    {
        // Dapr service discovery relies on gRPC reflection.
        app.MapGrpcReflectionService();

        // TODO: Is this really necessary (it's from the MSDN docs)?
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        return app;
    }

    public static GrpcServiceEndpointConventionBuilder MapOutputBinding<T>(this IEndpointRouteBuilder app) where T : Proto.Components.V1.OutputBinding.OutputBindingBase
    {
        return app.MapGrpcService<T>();
    }

    public static GrpcServiceEndpointConventionBuilder MapPubSub<T>(this IEndpointRouteBuilder app) where T : Proto.Components.V1.PubSub.PubSubBase
    {
        return app.MapGrpcService<T>();
    }

    public static GrpcServiceEndpointConventionBuilder MapStateStore<T>(this IEndpointRouteBuilder app) where T : Proto.Components.V1.StateStore.StateStoreBase
    {
        return app.MapGrpcService<T>();
    }
}