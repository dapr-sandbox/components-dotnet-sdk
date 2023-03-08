// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System.Collections.Concurrent;
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

/// <summary>
/// Represents the gRPC protocol adaptor for a pub-sub Dapr Pluggable Component.
/// </summary>
/// <remarks>
/// An instances of this class is created for every request made to the component.
/// </remarks>
public class PubSubAdaptor : PubSubBase
{
    private readonly ILogger<PubSubAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IPubSub> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="PubSubAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public PubSubAdaptor(ILogger<PubSubAdaptor> logger, IDaprPluggableComponentProvider<IPubSub> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext ctx)
    {
        this.logger.LogDebug("Features request");

        var response = new FeaturesResponse();

        if (this.GetPubSub(ctx) is IPluggableComponentFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(ctx.CancellationToken);

            response.Features.AddRange(featuresResponse);
        }

        return response;
    }

    /// <inheritdoc/>
    public async override Task<PubSubInitResponse> Init(Proto.Components.V1.PubSubInitRequest request, ServerCallContext ctx)
    {
        this.logger.LogDebug("Init request");

        await this.GetPubSub(ctx).InitAsync(
            Components.MetadataRequest.FromMetadataRequest(request.Metadata),
            ctx.CancellationToken);

        return new PubSubInitResponse();
    }

    /// <inheritdoc/>
    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogDebug("Ping request");

        if (this.GetPubSub(ctx) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(ctx.CancellationToken);
        }

        return new PingResponse();
    }

    /// <inheritdoc/>
    public override async Task<PublishResponse> Publish(PublishRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Publish request");

        await this.GetPubSub(context).PublishAsync(
            PubSubPublishRequest.FromPublishRequest(request),
            context.CancellationToken);

        return new PublishResponse();
    }

    /// <inheritdoc/>
    public override async Task PullMessages(IAsyncStreamReader<PullMessagesRequest> requests, IServerStreamWriter<PullMessagesResponse> responses, ServerCallContext context)
    {
        this.logger.LogDebug("Pull messages request");

        // The first request should contain the topic from which to pull messages.
        if (!await requests.MoveNext(context.CancellationToken))
        {
            throw new InvalidOperationException(Resources.PubSubAdaptorPullExpectsRequestMessage);
        }

        var topic = requests.Current.Topic;

        if (topic == null || String.IsNullOrEmpty(topic.Name))
        {
            throw new InvalidOperationException(Resources.PubSubAdaptorPullExpectsTopicMessage);
        }

        var pendingMessages = new ConcurrentDictionary<string, MessageAcknowledgementHandler<string?>>();

        var acknowledgeTask =
            async () =>
            {
                while (await requests.MoveNext(context.CancellationToken))
                {
                    var request = requests.Current;

                    if (request.AckMessageId != null && pendingMessages.TryRemove(request.AckMessageId, out var response))
                    {
                        await response(request.AckError?.Message);
                    }
                }
            };

        var pullTask = this.GetPubSub(context).PullMessagesAsync(
            PubSubPullMessagesTopic.FromTopic(topic),
            async (message, onAcknowledgement) =>
            {
                string messageId = Guid.NewGuid().ToString();

                var response = PubSubPullMessagesResponse.ToPullMessagesResponse(messageId, message);

                if (onAcknowledgement != null)
                {
                    pendingMessages[messageId] = onAcknowledgement;
                }

                try
                {
                    await responses.WriteAsync(response);
                }
                catch
                {
                    // If unable to write the message, there shouldn't be an acknowledgement.
                    pendingMessages.TryRemove(messageId, out var _);

                    throw;
                }
            },
            context.CancellationToken);

        await Task.WhenAll(acknowledgeTask(), pullTask);
    }

    private IPubSub GetPubSub(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}
