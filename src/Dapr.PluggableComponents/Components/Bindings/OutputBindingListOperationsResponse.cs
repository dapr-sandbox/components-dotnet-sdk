namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class OutputBindingListOperationsResponse
{
    public string[] Operations { get; init; } = Array.Empty<string>();
}
