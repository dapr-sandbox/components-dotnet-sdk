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

namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class PubSubPullMessagesTopicTests
{
    [Fact]
    public void FromTopicTests()
    {
        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var topic = new Topic();

                topic.Metadata.Add(metadata);

                return topic;
            },
            PubSubPullMessagesTopic.FromTopic,
            topic => topic.Metadata);

        ConversionAssert.StringEqual(
            name => new Topic { Name = name },
            PubSubPullMessagesTopic.FromTopic,
            topic => topic.Name);
    }
}
