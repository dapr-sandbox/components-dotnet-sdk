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
using Dapr.PluggableComponents.Components.SecretStores;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.SecretStore;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents the gRPC protocol adaptor for a state store Dapr Pluggable Component.
/// </summary>
/// <remarks>
/// An instances of this class is created for every request made to the component.
/// </remarks>
public class SecretStoreAdaptor : SecretStoreBase
{
    private readonly ILogger<SecretStoreAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<ISecretStore> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="SecretStoreAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public SecretStoreAdaptor(ILogger<SecretStoreAdaptor> logger, IDaprPluggableComponentProvider<ISecretStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<FeaturesResponse> Features(FeaturesRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Features request");

        var response = new FeaturesResponse();

        if (this.GetSecretStore(context) is IPluggableComponentFeatures features)
        {
            var featuresResponse = await features.GetFeaturesAsync(context.CancellationToken);

            response.Features.AddRange(featuresResponse);
        }

        return response;
    }

    /// <inheritdoc/>
    public override async Task<GetSecretResponse> Get(GetSecretRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Get request for key {key}", request.Key);

        var response = await this.GetSecretStore(context).GetAsync(
            SecretStoreGetRequest.FromGetRequest(request),
            context.CancellationToken);

        return SecretStoreGetResponse.ToGetResponse(response);
    }

    /// <inheritdoc/>
    public override async Task<BulkGetSecretResponse> BulkGet(BulkGetSecretRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Bulk get request for secret");

        var secretStore = this.GetSecretStore(context);
        var response = await secretStore.BulkGetAsync(SecretStoreBulkGetRequest.FromGetRequest(request), context.CancellationToken);
        return SecretStoreBulkGetResponse.ToBulkGetResponse(response);
    }

    /// <inheritdoc/>
    public async override Task<SecretStoreInitResponse> Init(Proto.Components.V1.SecretStoreInitRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Init request");

        await this.GetSecretStore(context).InitAsync(
            Components.MetadataRequest.FromMetadataRequest(request.Metadata),
            context.CancellationToken);

        return new SecretStoreInitResponse();
    }

    /// <inheritdoc/>
    public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Ping request");

        if (this.GetSecretStore(context) is IPluggableComponentLiveness ping)
        {
            await ping.PingAsync(context.CancellationToken);
        }

        return new PingResponse();
    }

    private ISecretStore GetSecretStore(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}

