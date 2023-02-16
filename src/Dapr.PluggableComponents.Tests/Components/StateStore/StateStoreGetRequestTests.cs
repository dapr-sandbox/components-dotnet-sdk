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

public sealed class StateStoreGetRequestTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("key", "key")]
    public void FromGetRequestKeyTests(string key, string expectedKey)
    {
        var grpcRequest = new GetRequest
        {
            Key = key
        };

        var request = StateStoreGetRequest.FromGetRequest(grpcRequest);

        Assert.Equal(expectedKey, request.Key);
    }

    public static IEnumerable<object[]> MetadataTests =>
        new[]{
            new[]
            {
                new Dictionary<string, string>()
            },
            new[]
            {
                new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                }
            }
        };

    [Theory]
    [MemberData(nameof(MetadataTests))]
    public void FromGetRequestMetadataTests(IDictionary<string, string> metadata)
    {
        var grpcRequest = new GetRequest();

        grpcRequest.Metadata.Add(metadata);

        var request = StateStoreGetRequest.FromGetRequest(grpcRequest);

        Assert.Equal(metadata, request.Metadata);
    }

    [Theory]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyEventual, StateStoreConsistency.Eventual)]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyStrong, StateStoreConsistency.Strong)]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyUnspecified, StateStoreConsistency.Unspecified)]
    public void FromGetRequestConsistencyTests(StateOptions.Types.StateConsistency grpcConsistency, StateStoreConsistency expectedConsistency)
    {
        var grpcRequest = new GetRequest
        {
            Consistency = grpcConsistency
        };

        var request = StateStoreGetRequest.FromGetRequest(grpcRequest);

        Assert.Equal(expectedConsistency, request.Consistency);
    }
}