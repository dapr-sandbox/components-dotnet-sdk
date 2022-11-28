using Microsoft.AspNetCore.Builder;

namespace Dapr.PluggableComponents;

public sealed record DaprPluggableComponentOptions
{
    public string? SocketExtension { get; init; }

    public string? SocketFolder { get; init; }

    public string? SocketName { get; init; }

    public Action<WebApplication>? WebApplicationConfiguration { get; init; }

    public Action<WebApplicationBuilder>? WebApplicationBuilderConfiguration { get; init; }
}
