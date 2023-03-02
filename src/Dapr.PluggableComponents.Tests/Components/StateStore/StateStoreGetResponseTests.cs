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

public sealed class StateStoreGetResponseTests
{
    [Fact]
    public void ToGetResponseNull()
    {
        var response = StateStoreGetResponse.ToGetResponse(null);

        Assert.Equal(String.Empty, response.ContentType);
        Assert.Empty(response.Data);
        Assert.Null(response.Etag);
        Assert.Empty(response.Metadata);
    }

    [Fact]
    public void ToGetResponseTests()
    {
        var converter = StateStoreGetResponse.ToGetResponse;

        ConversionAssert.ContentTypeEqual(
            contentType => new StateStoreGetResponse { ContentType = contentType },
            converter,
            response => response.ContentType);

        ConversionAssert.DataEqual(
            data => new StateStoreGetResponse { Data = data },
            converter,
            response => response.Data);

        ConversionAssert.ETagEqual(
            etag => new StateStoreGetResponse { ETag = etag },
            converter,
            response => response.Etag);

        ConversionAssert.MetadataEqual(
            metadata => new StateStoreGetResponse { Metadata = metadata },
            converter,
            response => response.Metadata);
    }

    [Fact]
    public void ToBulkStateItemTests()
    {
        var converter = StateStoreGetResponse.ToBulkStateItem;

        ConversionAssert.Equal(
            key => new StateStoreGetResponse(),
            converter,
            result => result.Key,
            new[]
            {
                ("", ""),
                ("key", "key")
            });

        Func<StateStoreGetResponse, Proto.Components.V1.BulkStateItem> keylessConverter = response => converter("key", response);

        ConversionAssert.ContentTypeEqual(
            contentType => new StateStoreGetResponse { ContentType = contentType },
            keylessConverter,
            response => response.ContentType);

        ConversionAssert.DataEqual(
            data => new StateStoreGetResponse { Data = data },
            keylessConverter,
            response => response.Data);

        ConversionAssert.ETagEqual(
            etag => new StateStoreGetResponse { ETag = etag },
            keylessConverter,
            response => response.Etag);

        ConversionAssert.MetadataEqual(
            metadata => new StateStoreGetResponse { Metadata = metadata },
            keylessConverter,
            response => response.Metadata);
    }

    [Fact]
    public void ToBulkStateItemWithNull()
    {
        var response = StateStoreGetResponse.ToBulkStateItem("key", null);

        Assert.Equal(String.Empty, response.ContentType);
        Assert.Empty(response.Data);
        Assert.Null(response.Etag);
        Assert.NotNull(response.Error);
        Assert.Empty(response.Metadata);
    }
}
