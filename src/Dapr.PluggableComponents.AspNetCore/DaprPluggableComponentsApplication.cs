using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplication
{
    private readonly string[] args;
    private readonly DaprPluggableComponentOptions options;

    private readonly ConcurrentBag<Action<WebApplicationBuilder>> builderActions = new ConcurrentBag<Action<WebApplicationBuilder>>();
    private readonly ConcurrentBag<Action<WebApplication>> appActions = new ConcurrentBag<Action<WebApplication>>();

    private DaprPluggableComponentsApplication(string[] args, DaprPluggableComponentOptions options)
    {       
        this.args = args;
        this.options = options;
    }

    public static DaprPluggableComponentsApplication Create(string[] args, DaprPluggableComponentOptions options)
    {
        return new DaprPluggableComponentsApplication(args, options);
    }

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
        var builder = WebApplication.CreateBuilder(args);

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