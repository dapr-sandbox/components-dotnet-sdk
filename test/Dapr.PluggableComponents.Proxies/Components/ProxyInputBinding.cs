using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Proxies.Grpc.v1;
using Dapr.PluggableComponents.Proxies.Utilities;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Proxies.Components;

internal sealed class ProxyInputBinding : IInputBinding, IPluggableComponentLiveness
{
    private readonly IGrpcChannelProvider grpcChannelProvider;
    private readonly ILogger<ProxyInputBinding> logger;

    public ProxyInputBinding(IGrpcChannelProvider grpcChannelProvider, ILogger<ProxyInputBinding> logger)
    {
        this.grpcChannelProvider = grpcChannelProvider ?? throw new ArgumentNullException(nameof(grpcChannelProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IInputBinding Members

    // TODO: How is initialization handled when a binding is both and input *and* output binding?
    public async Task InitAsync(Dapr.PluggableComponents.Components.MetadataRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Init request");

        var grpcRequest = new Dapr.PluggableComponents.Proxies.Grpc.v1.InputBindingInitRequest
        {
            Metadata = new Dapr.PluggableComponents.Proxies.Grpc.v1.MetadataRequest()
        };

        grpcRequest.Metadata.Properties.Add(request.Properties);

        await this.GetClient().InitAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task ReadAsync(IAsyncEnumerable<InputBindingReadRequest> requests, IAsyncMessageWriter<InputBindingReadResponse> responses, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Read request");

        using var stream = this.GetClient().Read(cancellationToken: cancellationToken);

        var requestReaderTask =
            async () =>
            {
                await foreach (var request in requests.WithCancellation(cancellationToken))
                {
                    var grpcRequest = new ReadRequest
                    {
                        ResponseError = request.ResponseErrorMessage != null ? new AckResponseError { Message = request.ResponseErrorMessage } : null,
                        MessageId = request.MessageId,
                        ResponseData = ByteString.CopyFrom(request.ResponseData.ToArray())
                    };

                    this.logger.LogInformation("Request received (ID = {0})", request.MessageId);

                    await stream.RequestStream.WriteAsync(grpcRequest, cancellationToken);
                }
            };

        var responseReaderTask =
            async () =>
            {
                await foreach (var response in stream.ResponseStream.AsEnumerable().WithCancellation(cancellationToken))
                {
                    this.logger.LogInformation("Response received (ID = {0})", response.MessageId);

                    await responses.WriteAsync(
                        new InputBindingReadResponse(response.MessageId)
                        {
                            ContentType = response.ContentType,
                            Data = response.Data.ToArray(),
                            Metadata = response.Metadata
                        },
                        cancellationToken);
                }
            };

        await Task.WhenAll(requestReaderTask(), responseReaderTask());
    }

    #endregion

    #region IPluggableComponentLiveness Members

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Ping request");

        await this.GetClient().PingAsync(
            new PingRequest(),
            cancellationToken: cancellationToken);
    }

    #endregion

    private InputBinding.InputBindingClient GetClient()
    {
        return new InputBinding.InputBindingClient(this.grpcChannelProvider.GetChannel());
    }
}