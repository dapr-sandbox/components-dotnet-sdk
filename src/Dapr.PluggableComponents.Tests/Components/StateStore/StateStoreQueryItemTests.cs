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

public sealed class StateStoreQueryItemTests
{
    [Fact]
    public void ToQueryItemConversionTests()
    {
        var converter = StateStoreQueryItem.ToQueryItem;

        ConversionAssert.ContentTypeEqual(
            contentType => new StateStoreQueryItem("key") { ContentType = contentType },
            converter,
            item => item.ContentType);

        ConversionAssert.DataEqual(
            data => new StateStoreQueryItem("key") { Data = data },
            converter,
            item => item.Data);

        ConversionAssert.NullableStringEqual(
            error => new StateStoreQueryItem("key") { Error = error },
            converter,
            item => item.Error);

        ConversionAssert.ETagEqual(
            etag => new StateStoreQueryItem("key") { ETag = etag },
            converter,
            item => item.Etag);

        ConversionAssert.StringEqual(
            key => new StateStoreQueryItem(key),
            converter,
            item => item.Key);
    }
}
