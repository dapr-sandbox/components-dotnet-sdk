namespace Dapr.PluggableComponents.Components;

public interface IAsyncMessageWriter<in T>
{
    Task WriteAsync(T message, CancellationToken cancellationToken = default);
}