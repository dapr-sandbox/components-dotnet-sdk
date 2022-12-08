namespace Dapr.PluggableComponents.Components;

public interface IPluggableComponentFeatures
{
    Task<string[]> GetFeaturesAsync(CancellationToken cancellationToken = default);
}
