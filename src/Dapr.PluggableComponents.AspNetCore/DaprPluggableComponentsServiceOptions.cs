namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsServiceOptions
{
    public DaprPluggableComponentsServiceOptions(string socketName)
    {
        if (String.IsNullOrEmpty(socketName))
        {
            throw new ArgumentException("A valid socket name must be specified.");
        }

        this.SocketName = socketName;
    }

    public string? SocketExtension { get; init; }

    public string? SocketFolder { get; init; }

    public string SocketName { get; }
}
