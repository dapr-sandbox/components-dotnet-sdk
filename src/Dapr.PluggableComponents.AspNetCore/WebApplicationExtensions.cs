using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dapr.PluggableComponents;

public static class WebApplicationExtensions
{
    public static TBuilder MapDaprPluggableComponentsSupportServices<TBuilder>(this TBuilder app)
        where TBuilder : IEndpointRouteBuilder
    {
        // Dapr component discovery relies on the gRPC reflection service.
        app.MapGrpcReflectionService();

        return app;
    }

    public static GrpcServiceEndpointConventionBuilder MapDaprPluggableComponentAdaptor<T>(this IEndpointRouteBuilder app)
        where T : class
        {
            return app.MapGrpcService<T>();
        }
}