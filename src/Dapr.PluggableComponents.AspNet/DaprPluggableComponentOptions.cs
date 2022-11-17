namespace Dapr.PluggableComponents;

public sealed record DaprPluggableComponentOptions
{
    public string? SocketExtension { get; init; }

    public string? SocketFolder { get; init; }

    public string? SocketName { get; init; }
}
