namespace Dapr.PluggableComponents.Components.Bindings;

public interface IInputBinding : IPluggableComponent
{
    Task ReadAsync(IAsyncEnumerable<InputBindingReadRequest> request, IAsyncMessageWriter<InputBindingReadResponse> response, CancellationToken cancellationToken = default);
}
