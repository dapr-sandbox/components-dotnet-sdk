namespace Dapr.PluggableComponents.Components.Bindings;

public interface IInputBinding : IPluggableComponent
{
    Task ReadAsync(IAsyncEnumerable<InputBindingReadRequest> requests, IAsyncMessageWriter<InputBindingReadResponse> responses, CancellationToken cancellationToken = default);
}
