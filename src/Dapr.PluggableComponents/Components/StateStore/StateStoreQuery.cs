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
using Google.Protobuf.WellKnownTypes;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents a query to be performed on a state store.
/// </summary>
public sealed record StateStoreQuery
{
    /// <summary>
    /// Gets properties related to how contents of the state store should be filtered and returned.
    /// </summary>
    public IReadOnlyDictionary<string, Any> Filter { get; init; } = new Dictionary<string, Any>();

    /// <summary>
    /// Gets properties related to how results of the query should be paginated, if any.
    /// </summary>
    public StateStoreQueryPagination? Pagination { get; init; }

    /// <summary>
    /// Gets the keys by which results of the query should be sorted, if any.
    /// </summary>
    public StateStoreQuerySorting[] Sorting { get; init; } = Array.Empty<StateStoreQuerySorting>();

    internal static StateStoreQuery? FromQuery(Query? query)
        => query != null
            ? new StateStoreQuery
            {
                Filter = query.Filter,
                Pagination = StateStoreQueryPagination.FromPagination(query.Pagination),
                Sorting = query.Sort.Select(StateStoreQuerySorting.FromSorting).ToArray()
            }
            : null;
}
