﻿// ------------------------------------------------------------------------
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

using Dapr.PluggableComponents.Components.StateStore;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.TransactionalStateStore;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents the gRPC protocol adaptor for a state store Dapr Pluggable Component that supports transactions.
/// </summary>
public class TransactionalStateStoreAdaptor : TransactionalStateStoreBase
{
    private readonly ILogger<TransactionalStateStoreAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<ITransactionalStateStore> componentProvider;

    /// <summary>
    /// Creates a new instance of the <see cref="TransactionalStateStoreAdaptor"/> class.
    /// </summary>
    /// <param name="logger">A logger used for internal purposes.</param>
    /// <param name="componentProvider">A means to obtain the Dapr Pluggable Component associated with this adapter instance.</param>
    /// <exception cref="ArgumentNullException">If any parameter is null.</exception>
    public TransactionalStateStoreAdaptor(ILogger<TransactionalStateStoreAdaptor> logger, IDaprPluggableComponentProvider<ITransactionalStateStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    /// <inheritdoc/>
    public override async Task<TransactionalStateResponse> Transact(TransactionalStateRequest request, ServerCallContext context)
    {
        this.logger.LogDebug("Transact request");

        await this.GetStateStore(context).TransactAsync(
            StateStoreTransactRequest.FromTransactionalStateRequest(request),
            context.CancellationToken);

        return new TransactionalStateResponse();
    }

    private ITransactionalStateStore GetStateStore(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }
}
