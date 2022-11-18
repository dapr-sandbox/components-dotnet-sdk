namespace Dapr.PluggableComponents.Components;

// TODO: Consider naming.
public interface IFeatures
{
    Task<string[]> GetFeaturesAsync(CancellationToken cancellationToken = default);
}
