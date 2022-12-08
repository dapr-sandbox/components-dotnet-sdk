namespace Dapr.PluggableComponents.Components;

public interface IPluggableComponentLiveness
{
    Task PingAsync(CancellationToken cancellationToken = default);
}
