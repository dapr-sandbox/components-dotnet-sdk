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
using Google.Protobuf;
using Xunit;

namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubPublishRequestTests
{
    [Fact]
    public void FromPublishRequestTests()
    {
        // NOTE: Conversion from gRPC request content type is not symmetric with the reverse,
        //       so one cannot use the ConversionAssert.ContentTypeEqual() method.
        ConversionAssert.Equal(
            contentType => new PublishRequest { ContentType = contentType },
            (_, request) => PubSubPublishRequest.FromPublishRequest(request),
            request => request.ContentType,
            new (string, string?)[]
            {
                ("", ""),
                ("application/json", "application/json")
            });

        ConversionAssert.DataEqual(
            data => new PublishRequest { Data = ByteString.CopyFrom(data) },
            request => PubSubPublishRequest.FromPublishRequest(request),
            request => request.Data.ToArray());

        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var request = new PublishRequest();

                request.Metadata.Add(metadata);

                return request;
            },
            request => PubSubPublishRequest.FromPublishRequest(request),
            request => request.Metadata);

        ConversionAssert.StringEqual(
            pubSubName => new PublishRequest { PubsubName = pubSubName },
            request => PubSubPublishRequest.FromPublishRequest(request),
            request => request.PubSubName);

        ConversionAssert.StringEqual(
            topic => new PublishRequest { Topic = topic },
            request => PubSubPublishRequest.FromPublishRequest(request),
            request => request.Topic);
    }
}