using Microsoft.AspNetCore.Builder;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplicationOptions
{
    public Action<WebApplication>? WebApplicationConfiguration { get; init; }

    public Action<WebApplicationBuilder>? WebApplicationBuilderConfiguration { get; init; }

    public WebApplicationOptions? WebApplicationOptions { get; init; }
}
