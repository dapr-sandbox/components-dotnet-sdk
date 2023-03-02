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
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreQueryRequestsTests
{
    [Fact]
    public void FromQueryRequestConversionTests()
    {
        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var request = new QueryRequest();

                request.Metadata.Add(metadata);

                return request;
            },
            StateStoreQueryRequest.FromQueryRequest,
            request => request.Metadata);

        var nonEmptyQuery = new Query();

        ConversionAssert.Equal(
            query => new QueryRequest { Query = query },
            (_, request) => StateStoreQueryRequest.FromQueryRequest(request),
            request => request.Query,
            new[]
            {
                (null, null),
                (new Query(), new StateStoreQuery())
            },
            (expected, actual) =>
            {
                if (expected != null)
                {
                    Assert.NotNull(actual);
                }
                else
                {
                    Assert.Null(actual);
                }
            });
    }
}
