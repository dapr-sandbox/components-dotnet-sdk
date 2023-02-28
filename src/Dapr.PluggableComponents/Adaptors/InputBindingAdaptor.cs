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
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.InputBinding;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents the gRPC protocol adaptor for an input binding Dapr Pluggable Component.
/// </summary>
/// <remarks>
/// An instances of this class is created for every request made to the component.
/// </remarks>
public class InputBindingAdaptor : InputBindingBase
{
    private readonly ILogger<InputBindingAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IInputBinding> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="InputBindingAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public InputBindingAdaptor(ILogger<InputBindingAdaptor> logger, IDaprPluggableComponentProvider<IInputBinding> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<InputBindingInitResponse> Init(Proto.Components.V1.InputBindingInitRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Init request");

        await this.GetInputBinding(context).InitAsync(
            Components.MetadataRequest.FromMetadataRequest(request.Metadata),
            context.CancellationToken);

        return new InputBindingInitResponse();
    }

    /// <inheritdoc/>
    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogDebug("Ping request");

        if (this.GetInputBinding(ctx) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    /// <inheritdoc/>
    public override async Task Read(IAsyncStreamReader<ReadRequest> requestStream, IServerStreamWriter<ReadResponse> responseStream, ServerCallContext context)
    {
        this.logger.LogDebug("Read request");

        var pendingMessages = new ConcurrentDictionary<string, MessageAcknowledgementHandler<InputBindingReadRequest>>();

        var acknowledeTask =
            async () =>
            {
                while (await requestStream.MoveNext(context.CancellationToken))
                {
                    var request = requestStream.Current;

                    if (request.MessageId != null && pendingMessages.TryRemove(request.MessageId, out var response))
                    {
                        await response(InputBindingReadRequest.FromReadRequest(request));
                    }
                }
            };

        var pullTask = this.GetInputBinding(context).ReadAsync(
            async (message, onAcknowledgement) =>
            {
                string messageId = Guid.NewGuid().ToString();

                var response = InputBindingReadResponse.ToReadResponse(messageId, message);

                if (onAcknowledgement != null)
                {
                    pendingMessages[messageId] = onAcknowledgement;
                }

                try
                {
                    await responseStream.WriteAsync(response);
                }
                catch
                {
                    // If unable to write the message, there shouldn't be an acknowledgement.
                    pendingMessages.TryRemove(messageId, out var _);

                    throw;
                }
            },
            context.CancellationToken);

        await Task.WhenAll(acknowledeTask(), pullTask);
    }

    private IInputBinding GetInputBinding(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}
