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
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreQuerySortingTests
{
    [Fact]
    public void FromSortingConversionTests()
    {
        var converter = StateStoreQuerySorting.FromSorting;
        StateStoreQuerySorting curriedConverter<T>(T _, Sorting request) => converter(request);

        ConversionAssert.StringEqual(
            key => new Sorting { Key = key },
            converter,
            sorting => sorting.Key);

        ConversionAssert.Equal(
            order => new Sorting { Order = order },
            curriedConverter,
            sorting => sorting.Order,
            new[]
            {
                (Sorting.Types.Order.Asc, StateStoreQuerySortingOrder.Ascending),
                (Sorting.Types.Order.Desc, StateStoreQuerySortingOrder.Descending)
            });
    }

    [Theory]
    [InlineData(Sorting.Types.Order.Asc, StateStoreQuerySortingOrder.Ascending)]
    [InlineData(Sorting.Types.Order.Desc, StateStoreQuerySortingOrder.Descending)]
    public void FromSortingOrderTests(Sorting.Types.Order order, StateStoreQuerySortingOrder expected)
    {
        var actual = StateStoreQuerySorting.FromSortingOrder(order);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FromSortingOrderTestsUnknown()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StateStoreQuerySorting.FromSortingOrder((Sorting.Types.Order)(-1)));
    }
}
