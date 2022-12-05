using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.InputBinding;

namespace Dapr.PluggableComponents.Adaptors;

public class InputBindingAdaptor : InputBindingBase
{
    private readonly ILogger<InputBindingAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IInputBinding> componentProvider;

    public InputBindingAdaptor(ILogger<InputBindingAdaptor> logger, IDaprPluggableComponentProvider<IInputBinding> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<InputBindingInitResponse> Init(Proto.Components.V1.InputBindingInitRequest request, ServerCallContext context)
    {
        await this.GetInputBinding(context.RequestHeaders).InitAsync(
            new Components.InitRequest
            {
                // TODO: Metadata.
            },
            context.CancellationToken);

        return new InputBindingInitResponse();
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Ping request");

        if (this.GetInputBinding(ctx.RequestHeaders) is IPing ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    public override async Task Read(IAsyncStreamReader<ReadRequest> requestStream, IServerStreamWriter<ReadResponse> responseStream, ServerCallContext context)
    {
        await this.GetInputBinding(context.RequestHeaders).ReadAsync(
            requestStream
                .ToAsyncEnumerable()
                .WithTransform(
                    request =>
                    {
                        return new InputBindingReadRequest(request.MessageId)
                        {
                            ResponseData = request.ResponseData.Memory,
                            ResponseErrorMessage = request.ResponseError?.Message
                        };
                    }),
            new ServerStreamWriterAdaptor<ReadResponse, InputBindingReadResponse>(
                responseStream,
                response =>
                {
                    var grpcResponse = new ReadResponse
                    {
                        ContentType = response.ContentType,
                        Data = ByteString.CopyFrom(response.Data ?? Array.Empty<byte>()),
                        MessageId = response.MessageId,
                    };

                    grpcResponse.Metadata.Add(response.Metadata);

                    return grpcResponse;
                }));
    }

    private IInputBinding GetInputBinding(Metadata metadata)
    {
        return this.componentProvider.GetComponent(key => metadata.Get(key));
    }
}