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

using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents properties associated with a request to retrieve state from a state store.
/// </summary>
/// <param name="Key">The key that should be retrieved.</param>
public sealed record StateStoreGetRequest(string Key)
{
    /// <summary>
    /// Gets the consistency level for the request.
    /// </summary>
    /// <remarks>
    /// By default, the consistency level is <see cref="StateStoreConsistency.Unspecified"/>.
    /// </remarks>
    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static StateStoreGetRequest FromGetRequest(GetRequest request)
    {
        return new StateStoreGetRequest(request.Key)
        {
            Consistency = (StateStoreConsistency)request.Consistency,
            Metadata = request.Metadata
        };
    }
}
