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
/// Represents properties associated with a response to a query operation on a state store.
/// </summary>
public sealed record StateStoreQueryResponse
{
    /// <summary>
    /// Gets or sets the items returned by the query.
    /// </summary>
    public StateStoreQueryItem[] Items { get; init; } = Array.Empty<StateStoreQueryItem>();

    /// <summary>
    /// Gets or sets the metadata associated with the request.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to an empty dictionary.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the pagination token used by subsequent requests to get the next set of items. 
    /// </summary>
    public string? Token { get; init; }

    internal static QueryResponse ToQueryResponse(StateStoreQueryResponse response)
    {
        var grpcResponse = new QueryResponse
        {
            Token = response.Token ?? String.Empty
        };

        grpcResponse.Items.AddRange(response.Items.Select(StateStoreQueryItem.ToQueryItem));
        grpcResponse.Metadata.Add(response.Metadata);

        return grpcResponse;
    }
}
