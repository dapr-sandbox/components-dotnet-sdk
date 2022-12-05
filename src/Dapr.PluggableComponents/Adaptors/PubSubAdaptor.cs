using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.PubSub;

namespace Dapr.PluggableComponents.Adaptors;

public class PubSubAdaptor : PubSubBase
{
    private readonly ILogger<PubSubAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IPubSub> componentProvider;

    public PubSubAdaptor(ILogger<PubSubAdaptor> logger, IDaprPluggableComponentProvider<IPubSub> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Features request");

        var response = new FeaturesResponse();

        if (this.GetPubSub(ctx.RequestHeaders) is IFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(ctx.CancellationToken).ConfigureAwait(false);
    
            response.Features.AddRange(featuresResponse);
        }

        return response;
    }

    public async override Task<PubSubInitResponse> Init(Proto.Components.V1.PubSubInitRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Init request");
        
        await this.GetPubSub(ctx.RequestHeaders).InitAsync(
            new Components.InitRequest
            {
                Metadata = new Components.MetadataRequest { Properties = request.Metadata.Properties },
            },
            ctx.CancellationToken).ConfigureAwait(false);
        
        return new PubSubInitResponse();
    }

    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogInformation("Ping request");

        if (this.GetPubSub(ctx.RequestHeaders) is IPing ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    public override async Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        this.logger.LogInformation("Publish request");

        await this.GetPubSub(context.RequestHeaders).PublishAsync(
            new PubSubPublishRequest(request.PubsubName, request.Topic)
            {
                ContentType = request.ContentType,
                Data = request.Data.Memory,
                Metadata = request.Metadata
            },
            context.CancellationToken).ConfigureAwait(false);

        return new PublishResponse();
    }

    public override Task PullMessages(IAsyncStreamReader<PullMessagesRequest> requestStream, IServerStreamWriter<PullMessagesResponse> responseStream, ServerCallContext context)
    {
        this.logger.LogInformation("Pull messages request");

        return this.GetPubSub(context.RequestHeaders).PullMessagesAsync(
            requestStream
                .ToAsyncEnumerable(context.CancellationToken)
                .WithTransform(
                    request => new PubSubPullMessagesRequest
                    {
                        AckMessageError = request.AckError?.Message,
                        AckMessageId = request.AckMessageId,
                        Topic = request.Topic != null ? new PubSubPullMessagesTopic(request.Topic.Name) { Metadata = request.Topic.Metadata } : null
                    },
                    context.CancellationToken),
            new ServerStreamWriterAdaptor<PullMessagesResponse, PubSubPullMessagesResponse>(
                responseStream,
                message =>
                {
                    var response = new PullMessagesResponse()
                    {
                        ContentType = message.ContentType,
                        Data = message.Data != null ? ByteString.CopyFrom(message.Data) : ByteString.Empty,
                        Id = message.MessageId,
                        TopicName = message.TopicName
                    };

                    response.Metadata.Add(message.Metadata);

                    return response;
                }),
            context.CancellationToken);
    }

    private IPubSub GetPubSub(Metadata metadata)
    {
        return this.componentProvider.GetComponent(key => metadata.Get(key));
    }
}