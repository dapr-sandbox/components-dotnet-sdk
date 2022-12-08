using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.OutputBinding;

namespace Dapr.PluggableComponents.Adaptors;

public class OutputBindingAdaptor : OutputBindingBase
{
    private readonly ILogger<OutputBindingAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IOutputBinding> componentProvider;

    public OutputBindingAdaptor(ILogger<OutputBindingAdaptor> logger, IDaprPluggableComponentProvider<IOutputBinding> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<OutputBindingInitResponse> Init(Proto.Components.V1.OutputBindingInitRequest request, ServerCallContext context)
    {
        await this.GetOutputBinding(context.RequestHeaders)
            .InitAsync(
                Components.MetadataRequest.FromMetadataRequest(request.Metadata),
                context.CancellationToken);

        return new OutputBindingInitResponse();
    }

    public override async Task<InvokeResponse> Invoke(InvokeRequest request, ServerCallContext context)
    {
        var response = await this.GetOutputBinding(context.RequestHeaders).InvokeAsync(
            new OutputBindingInvokeRequest(request.Operation)
            {
                Data = request.Data.Memory,
                Metadata = request.Metadata
            },
            context.CancellationToken);

        var grpcResponse = new InvokeResponse
        {
            ContentType = response.ContentType,
            Data = ByteString.CopyFrom(response.Data ?? Array.Empty<byte>())
        };

        grpcResponse.Metadata.Add(response.Metadata);

        return grpcResponse;
    }

    public override async Task<ListOperationsResponse> ListOperations(ListOperationsRequest request, ServerCallContext context)
    {
        var operations = await this.GetOutputBinding(context.RequestHeaders).ListOperationsAsync(context.CancellationToken);

        var grpcResponse = new ListOperationsResponse();

        grpcResponse.Operations.Add(operations);

        return grpcResponse;
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Ping request");

        if (this.GetOutputBinding(ctx.RequestHeaders) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    private IOutputBinding GetOutputBinding(Metadata metadata)
    {
        return this.componentProvider.GetComponent(key => metadata.Get(key));
    }
}