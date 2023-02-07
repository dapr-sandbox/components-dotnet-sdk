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

using System.Globalization;
using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents the order in which query results can be sorted.
/// </summary>
public enum StateStoreQuerySortingOrder
{
    /// <summary>
    /// The results are to be sorted in ascending order.
    /// </summary>
    Ascending,

    /// <summary>
    /// The results are to be sorted in descending order.
    /// </summary>
    Descending
}

/// <summary>
/// Represents one ordering of query results.
/// </summary>
/// <param name="Key">Gets the state store key on which ordering is performed.</param>
public sealed record StateStoreQuerySorting(string Key)
{
    /// <summary>
    /// Gets the direction in which query results should be sorted, based on the specified key.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="StateStoreQuerySortingOrder.Ascending"/>.
    /// </remarks>
    public StateStoreQuerySortingOrder Order { get; init; } = StateStoreQuerySortingOrder.Ascending;

    internal static StateStoreQuerySorting FromSorting(Sorting sorting)
        => new StateStoreQuerySorting(sorting.Key)
        {
            Order = FromSortingOrder(sorting.Order)
        };

    internal static StateStoreQuerySortingOrder FromSortingOrder(Sorting.Types.Order order)
        => order switch
        {
            Sorting.Types.Order.Asc => StateStoreQuerySortingOrder.Ascending,
            Sorting.Types.Order.Desc => StateStoreQuerySortingOrder.Descending,
            _ => throw new ArgumentOutOfRangeException(nameof(order), String.Format(CultureInfo.CurrentCulture, "The sorting order \"{0}\" was not recognized.", order))
        };
}
