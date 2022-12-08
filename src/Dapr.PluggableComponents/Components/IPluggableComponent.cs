namespace Dapr.PluggableComponents.Components;

public interface IPluggableComponent
{
    Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default);
}
