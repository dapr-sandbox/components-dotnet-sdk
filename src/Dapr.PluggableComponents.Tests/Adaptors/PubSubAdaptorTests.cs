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

using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.Proto.Components.V1;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class PubSubAdaptorTests
{
    [Fact]
    public async Task InitTests()
    {
        var mockStateStore = new Mock<IPubSub>();

        mockStateStore
            .Setup(component => component.InitAsync(It.IsAny<Components.MetadataRequest>(), It.IsAny<CancellationToken>()));

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new Client.Autogen.Grpc.v1.MetadataRequest();

        metadataRequest.Properties.Add(properties);

        using var context = new TestServerCallContext();

        await adaptor.Init(
            new Proto.Components.V1.PubSubInitRequest
            {
                Metadata = metadataRequest
            },
            context);

        mockStateStore
            .Verify(
                component => component.InitAsync(
                    It.Is<Components.MetadataRequest>(request => ConversionAssert.MetadataEqual(properties, request.Properties)),
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }

    [Fact]
    public async Task PingWithNoLiveness()
    {
        var mockStateStore = new Mock<IPubSub>(MockBehavior.Strict);

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        using var context = new TestServerCallContext();

        await adaptor.Ping(
            new PingRequest(),
            context);
    }

    [Fact]
    public async Task PingWithLiveness()
    {
        var mockStateStore = new Mock<IPubSub>();
        var mockLiveness = mockStateStore.As<IPluggableComponentLiveness>();

        mockLiveness
            .Setup(component => component.PingAsync(It.IsAny<CancellationToken>()));

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        using var context = new TestServerCallContext();

        await adaptor.Ping(
            new PingRequest(),
            context);

        mockLiveness
            .Verify(
                component => component.PingAsync(
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }

    [Fact]
    public async Task FeaturesWithNoFeatures()
    {
        var mockStateStore = new Mock<IPubSub>(MockBehavior.Strict);

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        using var context = new TestServerCallContext();

        var response = await adaptor.Features(
            new FeaturesRequest(),
            context);

        Assert.NotNull(response);
        Assert.Empty(response.Features);
    }
    
    [Fact]
    public async Task FeaturesWithFeatures()
    {
        var mockStateStore = new Mock<IPubSub>();
        var mockFeatures = mockStateStore.As<IPluggableComponentFeatures>();

        mockFeatures
            .Setup(component => component.GetFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "feature1", "feature2" });

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        using var context = new TestServerCallContext();

        var response = await adaptor.Features(
            new FeaturesRequest(),
            context);

        Assert.NotNull(response);
        Assert.Equal(2, response.Features.Count);
        Assert.Contains("feature1", response.Features);
        Assert.Contains("feature2", response.Features);

        mockFeatures
            .Verify(
                component => component.GetFeaturesAsync(
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }

    [Fact]
    public async Task Publish()
    {
        var mockStateStore = new Mock<IPubSub>();

        mockStateStore
            .Setup(component => component.PublishAsync(It.IsAny<PubSubPublishRequest>(), It.IsAny<CancellationToken>()));

        var adaptor = AdaptorFixture.CreatePubSub(mockStateStore.Object);

        using var context = new TestServerCallContext();

        string topic = "topic";

        var response = await adaptor.Publish(
            new PublishRequest
            {
                Topic = topic,
            },
            context);

        mockStateStore
            .Verify(
                component => component.PublishAsync(
                    It.Is<PubSubPublishRequest>(request => request.Topic == topic),
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }
}
