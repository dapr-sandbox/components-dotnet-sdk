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

public sealed class StateStoreBulkStateItemTests
{
    [Fact]
    public void ToBulkStateItemKeyTests()
    {
        string key = "key";

        var response = new StateStoreBulkStateItem(key);

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Equal(key, grpcResponse.Key);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("application/json", "application/json")]
    public void ToBulkStateItemContentTypeTests(string? contentType, string expectedContentType)
    {
        var response = new StateStoreBulkStateItem("key")
        {
            ContentType = contentType
        };

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Equal(expectedContentType, grpcResponse.ContentType);
    }

    public static IEnumerable<object[]> DataTests =>
        new[]{
            new[]
            {
                new byte[] {}
            },
            new[]
            {
                new byte[] { 0x01, 0x02, 0x03 }
            }
        };

    [Theory]
    [MemberData(nameof(DataTests))]
    public void ToBulkStateItemDataTests(byte[] data)
    {
        var response = new StateStoreBulkStateItem("key")
        {
            Data = data
        };

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Equal(data, grpcResponse.Data);
    }

    [Fact]
    public void ToBulkStateItemETagTests()
    {
        var response = new StateStoreBulkStateItem("key");

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Null(grpcResponse.Etag);

        string etag = "value";

        response = new StateStoreBulkStateItem("key")
        {
            ETag = etag
        };

        grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.NotNull(grpcResponse.Etag);
        Assert.Equal(etag, grpcResponse.Etag.Value);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("error", "error")]
    public void ToBulkStateItemErrorTests(string? error, string expectedError)
    {
        var response = new StateStoreBulkStateItem("key")
        {
            Error = error
        };

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Equal(expectedError, grpcResponse.Error);
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
    public void ToBulkStateItemMetadataTests(Dictionary<string, string> metadata)
    {
        var response = new StateStoreBulkStateItem("key")
        {
            Metadata = metadata
        };

        var grpcResponse = StateStoreBulkStateItem.ToBulkStateItem(response);

        Assert.Equal(metadata, grpcResponse.Metadata);
    }
}
