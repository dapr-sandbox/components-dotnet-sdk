namespace Dapr.PluggableComponents.Components.Bindings;

public sealed record InputBindingReadRequest(string MessageId)
{
    public ReadOnlyMemory<byte> ResponseData { get; init; } = ReadOnlyMemory<byte>.Empty;

    public string? ResponseErrorMessage { get; init; }
}
