namespace Dapr.PluggableComponents.Components.Bindings;

public interface IInputBinding
{
    Task InitAsync(InputBindingInitRequest request, CancellationToken cancellationToken = default);

    Task ReadAsync(IAsyncEnumerable<InputBindingReadRequest> request, IAsyncMessageWriter<InputBindingReadResponse> response, CancellationToken cancellationToken = default);
}
