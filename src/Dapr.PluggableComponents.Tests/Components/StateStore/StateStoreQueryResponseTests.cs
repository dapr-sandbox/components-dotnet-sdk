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

using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreQueryResponseTests
{
    [Fact]
    public void ToQueryResponseConversionTests()
    {
        IEnumerable<string> empty = new string[] { };
        IEnumerable<string> nonEmpty = new[] { "key1", "key2" };

        var converter = StateStoreQueryResponse.ToQueryResponse;

        ConversionAssert.Equal(
            items => new StateStoreQueryResponse { Items = items.Select(item => new StateStoreQueryItem(item)).ToArray() },
            (_, response) => converter(response),
            response => response.Items.Select(item => item.Key),
            new[]
            {
                (empty, empty),
                (nonEmpty, nonEmpty)
            });

        ConversionAssert.NullableStringEqual(
            token => new StateStoreQueryResponse { Token = token },
            converter,
            response => response.Token);

        ConversionAssert.MetadataEqual(
            metadata => new StateStoreQueryResponse { Metadata = metadata },
            converter,
            response => response.Metadata);
    }
}
