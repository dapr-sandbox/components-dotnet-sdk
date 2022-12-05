namespace Dapr.PluggableComponents.Components.Bindings;

public sealed record InputBindingReadResponse(string MessageId)
{
    public byte[] Data { get; init; } = Array.Empty<byte>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string? ContentType { get; init; }
}
