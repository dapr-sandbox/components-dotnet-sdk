using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;

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