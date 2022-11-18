namespace Dapr.PluggableComponents.Components;

// TODO: Consider naming.
public interface IPing
{
    Task PingAsync(CancellationToken cancellationToken = default);
}
