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

using System.Threading.Channels;
using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class PubSubAdaptorTests
{
    private const int TimeoutInMs = 10000;

    [Fact]
    public Task InitTests()
    {
        return AdaptorFixture.TestInitAsync(
            () => AdaptorFixture.CreatePubSub(),
            (fixture, metadataRequest) => fixture.Adaptor.Init(new Proto.Components.V1.PubSubInitRequest { Metadata = metadataRequest }, fixture.Context));
    }

    [Fact]
    public Task PingTests()
    {
        return AdaptorFixture.TestPingAsync<PubSubAdaptor, IPubSub>(
            AdaptorFixture.CreatePubSub,
            fixture => fixture.Adaptor.Ping(new PingRequest(), fixture.Context));
    }

    [Fact]
    public async Task FeaturesWithNoFeatures()
    {
        using var fixture = AdaptorFixture.CreatePubSub(new Mock<IPubSub>(MockBehavior.Strict));

        var response = await fixture.Adaptor.Features(
            new FeaturesRequest(),
            fixture.Context);

        Assert.NotNull(response);
        Assert.Empty(response.Features);
    }

    [Fact]
    public async Task FeaturesWithFeatures()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        var mockFeatures = fixture.MockComponent.As<IPluggableComponentFeatures>();

        mockFeatures
            .Setup(component => component.GetFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "feature1", "feature2" });

        var response = await fixture.Adaptor.Features(
            new FeaturesRequest(),
            fixture.Context);

        Assert.NotNull(response);
        Assert.Equal(2, response.Features.Count);
        Assert.Contains("feature1", response.Features);
        Assert.Contains("feature2", response.Features);

        mockFeatures
            .Verify(
                component => component.GetFeaturesAsync(
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    [Fact]
    public async Task Publish()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .Setup(component => component.PublishAsync(It.IsAny<PubSubPublishRequest>(), It.IsAny<CancellationToken>()));

        string topic = "topic";

        var response = await fixture.Adaptor.Publish(
            new PublishRequest
            {
                Topic = topic,
            },
            fixture.Context);

        fixture.MockComponent
            .Verify(
                component => component.PublishAsync(
                    It.Is<PubSubPublishRequest>(request => request.Topic == topic),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesWithTopic()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .Setup(component => component.PullMessagesAsync(It.IsAny<PubSubPullMessagesTopic>(), It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), It.IsAny<CancellationToken>()));

        string topic = "topic";

        var mockWriter = new Mock<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter.Object,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        reader.Complete();

        await pullTask;

        fixture.MockComponent
            .Verify(
                component => component.PullMessagesAsync(
                    It.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                    It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesWithTopicAndMessage()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        string contentType = "application/json";
        string error = "error";

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        fixture.MockComponent
            .Setup(component => component.PullMessagesAsync(It.IsAny<PubSubPullMessagesTopic>(), It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), It.IsAny<CancellationToken>()))
            .Returns<PubSubPullMessagesTopic, MessageDeliveryHandler<string?, PubSubPullMessagesResponse>, CancellationToken>(
                async (topic, deliveryHandler, cancellationToken) =>
                {
                    await deliveryHandler(
                        new PubSubPullMessagesResponse(topic.Name) { ContentType = contentType },
                        ackError =>
                        {
                            Assert.Equal(error, ackError);

                            reader.Complete();

                            return Task.CompletedTask;
                        });
                });

        string topic = "topic";

        var mockWriter = new Mock<IServerStreamWriter<PullMessagesResponse>>();

        mockWriter
            .Setup(writer => writer.WriteAsync(It.IsAny<PullMessagesResponse>()))
            .Returns<PullMessagesResponse>(
                async response =>
                {
                    Assert.Equal(contentType, response.ContentType);
                    Assert.Equal(topic, response.TopicName);

                    await reader.AddAsync(new PullMessagesRequest { AckError = new AckMessageError { Message = error }, AckMessageId = response.Id });
                });

        var pullMessagesTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter.Object,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        await pullMessagesTask;

        fixture.MockComponent
            .Verify(
                component => component.PullMessagesAsync(
                    It.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                    It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    [Fact(Timeout = TimeoutInMs, Skip = "To be re-enabled when as part of fix for dapr-sandbox/components-dotnet-sdk#28.")]
    public async Task PullMessagesNoMessages()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .Setup(component => component.PullMessagesAsync(It.IsAny<PubSubPullMessagesTopic>(), It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), It.IsAny<CancellationToken>()));

        string topic = "topic";

        var mockWriter = new Mock<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter.Object,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        await pullTask;

        fixture.MockComponent
            .Verify(
                component => component.PullMessagesAsync(
                    It.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                    It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesNoRequests()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .Setup(component => component.PullMessagesAsync(It.IsAny<PubSubPullMessagesTopic>(), It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), It.IsAny<CancellationToken>()));

        var reader = new AsyncStreamReader<PullMessagesRequest>();
        var mockWriter = new Mock<IServerStreamWriter<PullMessagesResponse>>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter.Object,
            fixture.Context);

        reader.Complete();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await pullTask;
            }
        );
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesNoInitialTopic()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .Setup(component => component.PullMessagesAsync(It.IsAny<PubSubPullMessagesTopic>(), It.IsAny<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), It.IsAny<CancellationToken>()));

        var reader = new AsyncStreamReader<PullMessagesRequest>();
        var mockWriter = new Mock<IServerStreamWriter<PullMessagesResponse>>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter.Object,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { });

        reader.Complete();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await pullTask;
            }
        );
    }
}
