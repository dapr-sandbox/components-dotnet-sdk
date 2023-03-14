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

using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class OutputBindignAdaptorTests
{
    [Fact]
    public async Task InitTests()
    {
        using var fixture = AdaptorFixture.CreateOutputBinding();

        fixture.MockComponent
            .Setup(component => component.InitAsync(It.IsAny<Components.MetadataRequest>(), It.IsAny<CancellationToken>()));

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new Client.Autogen.Grpc.v1.MetadataRequest();

        metadataRequest.Properties.Add(properties);

        await fixture.Adaptor.Init(
            new Proto.Components.V1.OutputBindingInitRequest
            {
                Metadata = metadataRequest
            },
            fixture.Context);

        fixture.MockComponent
            .Verify(
                component => component.InitAsync(
                    It.Is<Components.MetadataRequest>(request => ConversionAssert.MetadataEqual(properties, request.Properties)),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }
}
