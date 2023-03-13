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

namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubPullMessagesResponseTests
{
    [Fact]
    public void ToPullMessagesResponseTests()
    {
        ConversionAssert.ContentTypeEqual(
            contentType => new PubSubPullMessagesResponse("topic") { ContentType = contentType },
            response => PubSubPullMessagesResponse.ToPullMessagesResponse("id", response),
            response => response.ContentType);

        ConversionAssert.DataEqual(
            data => new PubSubPullMessagesResponse("topic") { Data = data },
            response => PubSubPullMessagesResponse.ToPullMessagesResponse("id", response),
            response => response.Data);

        ConversionAssert.MetadataEqual(
            metadata => new PubSubPullMessagesResponse("topic") { Metadata = metadata },
            response => PubSubPullMessagesResponse.ToPullMessagesResponse("id", response),
            response => response.Metadata);

        ConversionAssert.StringEqual(
            _ => new PubSubPullMessagesResponse("topic"),
            (id, response) => PubSubPullMessagesResponse.ToPullMessagesResponse(id, response),
            response => response.Id);
    }
}
