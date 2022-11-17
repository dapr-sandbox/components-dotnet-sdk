using Microsoft.AspNetCore.Builder;

namespace Dapr.PluggableComponents;

public static class DaprPluggableComponentsApplication
{
    public static WebApplicationBuilder CreateBuilder(string[] args, DaprPluggableComponentOptions options)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.UseDaprPluggableComponents(options);

        return builder;
    }

    public static WebApplication Create(string[] args, DaprPluggableComponentOptions options)
    {
        var builder = CreateBuilder(args, options);

        var app = builder.Build();

        app.MapDaprPluggableComponents();

        return app;
    }
}