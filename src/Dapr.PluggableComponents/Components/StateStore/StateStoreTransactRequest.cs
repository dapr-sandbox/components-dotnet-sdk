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

using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents properties associated with a request to perform operations within a transaction.
/// </summary>
public sealed record StateStoreTransactRequest
{
    /// <summary>
    /// Gets the operations to be performed as part of the transaction.
    /// </summary>
    public StateStoreTransactOperation[] Operations { get; init; } = Array.Empty<StateStoreTransactOperation>();

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static StateStoreTransactRequest FromTransactionalStateRequest(TransactionalStateRequest request)
    {
        return new StateStoreTransactRequest
        {
            Metadata = request.Metadata,
            Operations = request.Operations.Select(StateStoreTransactOperation.FromTransactionalStateOperation).ToArray()
        };
    }
}
