using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Proxies.Grpc.v1;
using Dapr.PluggableComponents.Proxies.Utilities;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Proxies.Components;

internal sealed class ProxyOutputBinding : IOutputBinding
{
    private readonly IGrpcChannelProvider grpcChannelProvider;
    private readonly ILogger<ProxyOutputBinding> logger;

    public ProxyOutputBinding(IGrpcChannelProvider grpcChannelProvider, ILogger<ProxyOutputBinding> logger)
    {
        this.grpcChannelProvider = grpcChannelProvider ?? throw new ArgumentNullException(nameof(grpcChannelProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IOutputBinding Members

    // TODO: How is initialization handled when a binding is both and input *and* output binding?
    public async Task InitAsync(Dapr.PluggableComponents.Components.MetadataRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Init request");

        var client = new OutputBinding.OutputBindingClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new Dapr.PluggableComponents.Proxies.Grpc.v1.OutputBindingInitRequest
        {
            Metadata = new Dapr.PluggableComponents.Proxies.Grpc.v1.MetadataRequest()
        };

        grpcRequest.Metadata.Properties.Add(request.Properties);

        await client.InitAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("Invoke request for operation {0}", request.Operation);

        var client = new OutputBinding.OutputBindingClient(this.grpcChannelProvider.GetChannel());

        var grpcRequest = new InvokeRequest
        {
            Data = ByteString.CopyFrom(request.Data.ToArray()),
            Operation = request.Operation
        };

        grpcRequest.Metadata.Add(request.Metadata);

        var grpcResponse = await client.InvokeAsync(
            grpcRequest,
            cancellationToken: cancellationToken);

        return new OutputBindingInvokeResponse
        {
            ContentType = grpcResponse.ContentType,
            Data = grpcResponse.Data.ToArray(),
            Metadata = grpcResponse.Metadata
        };
    }

    public async Task<string[]> ListOperationsAsync(CancellationToken cancellationToken = default)
    {
        this.logger?.LogInformation("List operations request");

        var client = new OutputBinding.OutputBindingClient(this.grpcChannelProvider.GetChannel());

        var response = await client.ListOperationsAsync(
            new ListOperationsRequest(),
            cancellationToken: cancellationToken);        

        return response.Operations.ToArray();
    }

    #endregion
}