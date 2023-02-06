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
/// Represents properties related to how query results should be paginated.
/// </summary>
public sealed record StateStoreQueryPagination
{
    /// <summary>
    /// Gets the maximum number of items that should be returned.
    /// </summary>
    public long Limit { get; init; }

    /// <summary>
    /// Gets the pagination token returned by the previous query, if any.
    /// </summary>
    /// <remarks>
    /// If set, the results of the current query should be a continuation of the previous query.
    /// </remarks>
    public string? Token { get; init; }

    internal static StateStoreQueryPagination? FromPagination(Pagination? pagination)
        => pagination != null
            ? new StateStoreQueryPagination
            {
                Limit = pagination.Limit,
                Token = pagination.Token
            }
            : null;
}
