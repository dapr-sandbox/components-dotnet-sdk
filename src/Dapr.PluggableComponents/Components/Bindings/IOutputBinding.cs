namespace Dapr.PluggableComponents.Components.Bindings;

public interface IOutputBinding : IPluggableComponent
{
    Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default);

    Task<string[]> ListOperationsAsync(CancellationToken cancellationToken = default);
}