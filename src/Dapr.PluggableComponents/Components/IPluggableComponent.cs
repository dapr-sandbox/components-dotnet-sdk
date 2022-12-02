namespace Dapr.PluggableComponents.Components;

public interface IPluggableComponent
{
    Task InitAsync(InitRequest request, CancellationToken cancellationToken = default);
}
