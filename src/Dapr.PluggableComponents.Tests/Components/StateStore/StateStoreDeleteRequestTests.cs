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

public sealed class StateStoreDeleteRequestTests
{
    [Fact]
    public void FromDeleteRequestETagTests()
    {
        var grpcRequest = new DeleteRequest
        {
            Etag = null
        };

        var request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

        Assert.Null(request.ETag);

        string etag = "value";

        grpcRequest = new DeleteRequest
        {
            Etag = new Etag { Value = etag }
        };

        request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

        Assert.Equal(etag, request.ETag);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("key", "key")]
    public void FromDeleteRequestKeyTests(string key, string expectedKey)
    {
        var grpcRequest = new DeleteRequest
        {
            Key = key
        };

        var request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

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
    public void FromDeleteRequestMetadataTests(IDictionary<string, string> metadata)
    {
        var grpcRequest = new DeleteRequest();

        grpcRequest.Metadata.Add(metadata);

        var request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

        Assert.Equal(metadata, request.Metadata);
    }

    [Fact]
    public void FromDeleteRequestOptionsTests()
    {
        var grpcRequest = new DeleteRequest();

        var request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

        Assert.Null(request.Options);

        grpcRequest = new DeleteRequest
        {
            Options = new StateOptions
            {
                Concurrency = StateOptions.Types.StateConcurrency.ConcurrencyFirstWrite,
                Consistency = StateOptions.Types.StateConsistency.ConsistencyEventual
            }
        };

        request = StateStoreDeleteRequest.FromDeleteRequest(grpcRequest);

        Assert.NotNull(request.Options);
        Assert.Equal(StateStoreConcurrency.FirstWrite, request.Options.Concurrency);
        Assert.Equal(StateStoreConsistency.Eventual, request.Options.Consistency);
    }
}