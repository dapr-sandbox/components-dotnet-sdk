namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class InputBindingReadResponse
{
    public byte[] Data { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string ContentType { get; init; }

    public string MessageId { get; init; }
}
