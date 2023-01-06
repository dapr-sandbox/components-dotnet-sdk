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
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.StateStore;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents the gRPC protocol adaptor for a state store Dapr Pluggable Component.
/// </summary>
/// <remarks>
/// An instances of this class is created for every request made to the component.
/// </remarks>
public class StateStoreAdaptor : StateStoreBase
{
    private readonly ILogger<StateStoreAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<IStateStore> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="StateStoreAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public StateStoreAdaptor(ILogger<StateStoreAdaptor> logger, IDaprPluggableComponentProvider<IStateStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<BulkDeleteResponse> BulkDelete(BulkDeleteRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("BulkDelete request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(context);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            await bulkStateStore.BulkDeleteAsync(
                request.Items.Select(StateStoreDeleteRequest.FromDeleteRequest).ToArray(),
                context.CancellationToken);
        }
        else
        {
            this.logger.LogDebug("Store does not support bulk operations; falling back to individual operations.");

            foreach (var item in request.Items)
            {
                await stateStore.DeleteAsync(StateStoreDeleteRequest.FromDeleteRequest(item), context.CancellationToken);
            }
        }

        return new BulkDeleteResponse();
    }

    /// <inheritdoc/>
    public override async Task<BulkGetResponse> BulkGet(BulkGetRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Bulk get request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(context);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            var items = await bulkStateStore.BulkGetAsync(
                request.Items.Select(StateStoreGetRequest.FromGetRequest).ToArray(),
                context.CancellationToken);

            var response = new BulkGetResponse
            {
                Got = items.Any(item => String.IsNullOrEmpty(item.Error))
            };

            response.Items.Add(items.Select(StateStoreBulkStateItem.ToBulkStateItem));

            return response;
        }
        else
        {
            this.logger.LogDebug("Store does not support bulk operations; falling back to individual operations.");

            var responses = new List<BulkStateItem>();

            foreach (var item in request.Items)
            {
                var response = await stateStore.GetAsync(StateStoreGetRequest.FromGetRequest(item), context.CancellationToken);

                var stateItem = new BulkStateItem
                {
                    Key = item.Key
                };

                if (response != null)
                {
                    stateItem.ContentType = response.ContentType;
                    stateItem.Data = ByteString.CopyFrom(response.Data);
                    stateItem.Etag = response.ETag != null ? new Etag { Value = response.ETag } : null;

                    stateItem.Metadata.Add(response.Metadata);
                }
                else
                {
                    stateItem.Error = "Unable to fetch the item.";
                }

                responses.Add(stateItem);
            }

            var grpcResponse = new BulkGetResponse
            {
                Got = responses.Any(item => String.IsNullOrEmpty(item.Error))
            };

            grpcResponse.Items.Add(responses);

            return grpcResponse;
        }
    }

    /// <inheritdoc/>
    public override async Task<BulkSetResponse> BulkSet(BulkSetRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("BulkSet request for {count} keys", request.Items.Count);

        var stateStore = this.GetStateStore(context);

        if (stateStore is IBulkStateStore bulkStateStore)
        {
            await bulkStateStore.BulkSetAsync(
                request.Items.Select(StateStoreSetRequest.FromSetRequest).ToArray(),
                context.CancellationToken);
        }
        else
        {
            this.logger.LogDebug("Store does not support bulk operations; falling back to individual operations.");

            foreach (var item in request.Items)
            {
                await stateStore.SetAsync(StateStoreSetRequest.FromSetRequest(item), context.CancellationToken);
            }
        }

        return new BulkSetResponse();
    }

    /// <inheritdoc/>
    public override async Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Delete request for key {key}", request.Key);

        await this.GetStateStore(context).DeleteAsync(
            StateStoreDeleteRequest.FromDeleteRequest(request),
            context.CancellationToken);

        return new DeleteResponse();
    }

    /// <inheritdoc/>
    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Features request");

        var response = new FeaturesResponse();

        if (this.GetStateStore(context) is IPluggableComponentFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(context.CancellationToken);

            response.Features.AddRange(featuresResponse);
        }

        return response;
    }

    /// <inheritdoc/>
    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Get request for key {key}", request.Key);

        var response = await this.GetStateStore(context).GetAsync(
            StateStoreGetRequest.FromGetRequest(request),
            context.CancellationToken);

        return StateStoreGetResponse.ToGetResponse(response);
    }

    /// <inheritdoc/>
    public async override Task<InitResponse> Init(Proto.Components.V1.InitRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Init request");

        await this.GetStateStore(context).InitAsync(
            Components.MetadataRequest.FromMetadataRequest(request.Metadata),
            context.CancellationToken);

        return new InitResponse();
    }

    /// <inheritdoc/>
    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Ping request");

        if (this.GetStateStore(context) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(context.CancellationToken);
        }

        return new PingResponse();
    }

    /// <inheritdoc/>
    public override async Task<SetResponse> Set(SetRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Set request for key {key}", request.Key);

        await this.GetStateStore(context).SetAsync(
            StateStoreSetRequest.FromSetRequest(request),
            context.CancellationToken);

        return new SetResponse();
    }

    private IStateStore GetStateStore(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}
