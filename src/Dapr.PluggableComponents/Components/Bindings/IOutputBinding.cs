namespace Dapr.PluggableComponents.Components.Bindings;

public interface IOutputBinding
{
    Task InitAsync(OutputBindingInitRequest request, CancellationToken cancellationToken = default);

    Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default);

    Task<OutputBindingListOperationsResponse> ListOperationsAsync(CancellationToken cancellationToken = default);
}