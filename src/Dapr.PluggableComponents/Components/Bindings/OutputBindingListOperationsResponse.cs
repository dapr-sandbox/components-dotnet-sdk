namespace Dapr.PluggableComponents.Components.Bindings;

public sealed record OutputBindingListOperationsResponse
{
    public string[] Operations { get; init; } = Array.Empty<string>();
}
