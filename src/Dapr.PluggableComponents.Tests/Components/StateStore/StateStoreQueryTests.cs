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
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreQueryTests
{
    [Fact]
    public void FromQueryConversionTests()
    {
        var converter = StateStoreQuery.FromQuery;

        string key1 = "key1";
        string key2 = "key2";

        var filter1 = new Any();
        var filter2 = new Any();

        IReadOnlyDictionary<string, Any> emptyFilters = new Dictionary<string, Any> { };
        IReadOnlyDictionary<string, Any> nonEmptyFilters = new Dictionary<string, Any>
        {
            { key1, filter1 },
            { key2, filter2 }
        };

        ConversionAssert.Equal(
            filter =>
            {
                if (filter != null)
                {
                    var query = new Query();

                    query.Filter.Add(filter);

                    return query;
                }
                else
                {
                    return null;
                }
            },
            (_, query) => converter(query),
            query => query?.Filter,
            new[]
            {
                (null, null),
                (emptyFilters, emptyFilters),
                (nonEmptyFilters, nonEmptyFilters)
            });

        ConversionAssert.Equal(
            pagination => new Query { Pagination = pagination },
            (_, query) => converter(query),
            query => query?.Pagination,
            new[]
            {
                (null, null),
                (new Pagination { Token = "token"}, new StateStoreQueryPagination { Token = "token" })
            });

        IEnumerable<string> empty = new string[] { };
        IEnumerable<string> nonEmpty = new[] { "key1", "key2" };

        ConversionAssert.Equal(
            sorting =>
            {
                if (sorting != null)
                {
                    var query = new Query();
                    query.Sort.Add(sorting.Select(item => new Sorting { Key = item }));
                    return query;
                }
                else
                {
                    return null;
                }
            },
            (_, query) => converter(query),
            query => query?.Sorting?.Select(item => item.Key),
            new[]
            {
                (null, null),
                (empty, empty),
                (nonEmpty, nonEmpty)
            });
    }
}
