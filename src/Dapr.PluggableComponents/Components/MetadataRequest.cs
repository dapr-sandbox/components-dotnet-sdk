namespace Dapr.PluggableComponents.Components;

public sealed class MetadataRequest
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}
