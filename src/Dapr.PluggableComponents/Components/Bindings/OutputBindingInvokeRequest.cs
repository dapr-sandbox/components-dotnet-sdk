namespace Dapr.PluggableComponents.Components.Bindings;

public sealed record OutputBindingInvokeRequest(string Operation)
{
    public ReadOnlyMemory<byte> Data { get; init; } = ReadOnlyMemory<byte>.Empty;

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
