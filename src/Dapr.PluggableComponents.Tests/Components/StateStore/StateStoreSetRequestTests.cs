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

public sealed class StateStoreSetRequestTests
{
    [Fact]
    public void FromSetRequestConversionTests()
    {
        var converter = StateStoreSetRequest.FromSetRequest;
        StateStoreSetRequest curriedConverter<T>(T _, SetRequest request) => converter(request);

        // NOTE: Conversion from gRPC request content type is not symmetric with the reverse,
        //       so one cannot use the ConversionAssert.ContentTypeEqual() method.
        ConversionAssert.Equal(
            contentType => new SetRequest { ContentType = contentType },
            curriedConverter,
            request => request.ContentType,
            new (string, string?)[]
            {
                ("", ""),
                ("application/json", "application/json")
            });

        ConversionAssert.Equal(
            etag => new SetRequest { Etag = etag },
            curriedConverter,
            request => request.ETag,
            new[]
            {
                (null, null),
                (new Etag { Value = "" }, ""),
                (new Etag { Value = "value" }, "value")
            });

        ConversionAssert.Equal(
            options => new SetRequest { Options = options },
            curriedConverter,
            request => request.Options,
            new[]
            {
                (null, null),
                (new StateOptions(), new StateStoreStateOptions())
            });

        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var request = new SetRequest();

                request.Metadata.Add(metadata);

                return request;
            },
            converter,
            request => request.Metadata);
    }
}
