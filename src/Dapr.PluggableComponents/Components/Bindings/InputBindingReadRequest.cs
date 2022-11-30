namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class AckResponseError
{
    public string Message { get; init; }
}

public sealed class InputBindingReadRequest
{
    public ReadOnlyMemory<byte> ResponseData { get; init; }

    public string MessageId { get; init; }

    public AckResponseError ResponseError { get; init; }
}
