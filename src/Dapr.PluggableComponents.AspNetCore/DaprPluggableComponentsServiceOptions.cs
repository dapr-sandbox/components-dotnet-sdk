namespace Dapr.PluggableComponents;

public sealed record DaprPluggableComponentsServiceOptions(string SocketName)
{
    public string? SocketExtension { get; init; }

    public string? SocketFolder { get; init; }
}
