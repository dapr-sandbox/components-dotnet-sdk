namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class OutputBindingInvokeRequest
{
    public ReadOnlyMemory<byte> Data { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? Operation { get; init; }
}
