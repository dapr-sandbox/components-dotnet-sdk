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

using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.OutputBinding;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents the gRPC protocol adaptor for an output binding Dapr Pluggable Component.
/// </summary>
/// <remarks>
/// An instances of this class is created for every request made to the component.
/// </remarks>
public class OutputBindingAdaptor : OutputBindingBase
{
    private readonly ILogger<OutputBindingAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IOutputBinding> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="OutputBindingAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public OutputBindingAdaptor(ILogger<OutputBindingAdaptor> logger, IDaprPluggableComponentProvider<IOutputBinding> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<OutputBindingInitResponse> Init(Proto.Components.V1.OutputBindingInitRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Init request");

        await this.GetOutputBinding(context)
            .InitAsync(
                Components.MetadataRequest.FromMetadataRequest(request.Metadata),
                context.CancellationToken);

        return new OutputBindingInitResponse();
    }

    /// <inheritdoc/>
    public override async Task<InvokeResponse> Invoke(InvokeRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Invoke request");

        var response = await this.GetOutputBinding(context).InvokeAsync(OutputBindingInvokeRequest.FromInvokeRequest(request), context.CancellationToken);

        return OutputBindingInvokeResponse.ToInvokeResponse(response);
    }

    /// <inheritdoc/>
    public override async Task<ListOperationsResponse> ListOperations(ListOperationsRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("ListOperations request");

        var operations = await this.GetOutputBinding(context).ListOperationsAsync(context.CancellationToken);

        var grpcResponse = new ListOperationsResponse();

        grpcResponse.Operations.Add(operations);

        return grpcResponse;
    }

    /// <inheritdoc/>
    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext ctx)
    {
        this.logger.LogDebug("Ping request");

        if (this.GetOutputBinding(ctx) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(ctx.CancellationToken).ConfigureAwait(false);
        }

        return new PingResponse();
    }

    private IOutputBinding GetOutputBinding(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}