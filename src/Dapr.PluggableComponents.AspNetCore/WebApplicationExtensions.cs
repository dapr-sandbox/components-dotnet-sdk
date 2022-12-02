using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dapr.PluggableComponents;

public static class WebApplicationExtensions
{
    public static TBuilder MapDaprPluggableComponentsServices<TBuilder>(this TBuilder app)
        where TBuilder : IEndpointRouteBuilder
    {
        // Dapr component discovery relies on the gRPC reflection service.
        app.MapGrpcReflectionService();

        return app;
    }

    public static GrpcServiceEndpointConventionBuilder MapDaprPluggableComponent<T>(this IEndpointRouteBuilder app)
        where T : class
        {
            return app.MapGrpcService<T>();
        }
}