namespace Dapr.PluggableComponents.Components;

public sealed record MetadataRequest
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}
