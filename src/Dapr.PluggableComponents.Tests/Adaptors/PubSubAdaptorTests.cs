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
using Grpc.Core;
using NSubstitute;
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
        using var fixture = AdaptorFixture.CreatePubSub();

        var response = await fixture.Adaptor.Features(
            new FeaturesRequest(),
            fixture.Context);

        Assert.NotNull(response);
        Assert.Empty(response.Features);
    }

    [Fact]
    public async Task FeaturesWithFeatures()
    {
        using var fixture = AdaptorFixture.CreatePubSub(Substitute.For<IPubSub, IPluggableComponentFeatures>());

        var mockFeatures = (IPluggableComponentFeatures)fixture.MockComponent;

        mockFeatures
            .GetFeaturesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { "feature1", "feature2" });

        var response = await fixture.Adaptor.Features(
            new FeaturesRequest(),
            fixture.Context);

        Assert.NotNull(response);
        Assert.Equal(2, response.Features.Count);
        Assert.Contains("feature1", response.Features);
        Assert.Contains("feature2", response.Features);

        await mockFeatures
            .Received(1)
            .GetFeaturesAsync(
                Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }

    [Fact]
    public async Task Publish()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PublishAsync(Arg.Any<PubSubPublishRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        string topic = "topic";

        var response = await fixture.Adaptor.Publish(
            new PublishRequest
            {
                Topic = topic,
            },
            fixture.Context);

        await fixture.MockComponent
            .Received(1)
            .PublishAsync(
                Arg.Is<PubSubPublishRequest>(request => request.Topic == topic),
                Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesWithTopic()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        string topic = "topic";

        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        reader.Complete();

        await pullTask;

        await fixture.MockComponent
            .Received(1)
            .PullMessagesAsync(
                Arg.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                // NOTE: The adaptor provides its own cancellation token.
                Arg.Any<CancellationToken>());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesWithTopicAndMessage()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        string contentType = "application/json";
        string error = "error";

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(
                async callInfo =>
                {
                    var topic = callInfo.ArgAt<PubSubPullMessagesTopic>(0);
                    var deliveryHandler = callInfo.ArgAt<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(1);

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

        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        mockWriter
            .WriteAsync(Arg.Any<PullMessagesResponse>())
            .Returns(
                async callInfo =>
                {
                    var response = callInfo.ArgAt<PullMessagesResponse>(0);
                    Assert.Equal(contentType, response.ContentType);
                    Assert.Equal(topic, response.TopicName);

                    await reader.AddAsync(new PullMessagesRequest { AckError = new AckMessageError { Message = error }, AckMessageId = response.Id });
                });

        var pullMessagesTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        await pullMessagesTask;

        await fixture.MockComponent
            .Received(1)
            .PullMessagesAsync(
                Arg.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                // NOTE: The adaptor provides its own cancellation token.
                Arg.Any<CancellationToken>());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesNoMessages()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        string topic = "topic";

        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        await pullTask;

        await fixture.MockComponent
            .Received(1)
            .PullMessagesAsync(
                Arg.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                // NOTE: The adaptor provides its own cancellation token.
                Arg.Any<CancellationToken>());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesClientHasNoMoreMessages()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    return Task.Delay(-1, callInfo.ArgAt<CancellationToken>(2));
                });

        string topic = "topic";

        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        reader.Complete();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pullTask);

        await fixture.MockComponent
            .Received(1)
            .PullMessagesAsync(
                Arg.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                // NOTE: The adaptor provides its own cancellation token.
                Arg.Any<CancellationToken>());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesClientCanceled()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    return Task.Delay(-1, callInfo.ArgAt<CancellationToken>(2));
                });

        string topic = "topic";

        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var reader = new AsyncStreamReader<PullMessagesRequest>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
            fixture.Context);

        await reader.AddAsync(new PullMessagesRequest { Topic = new Topic { Name = topic } });

        fixture.Context.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pullTask);

        await fixture.MockComponent
            .Received(1)
            .PullMessagesAsync(
                Arg.Is<PubSubPullMessagesTopic>(request => request.Name == topic),
                Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(),
                // NOTE: The adaptor provides its own cancellation token.
                Arg.Any<CancellationToken>());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task PullMessagesNoRequests()
    {
        using var fixture = AdaptorFixture.CreatePubSub();

        fixture.MockComponent
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var reader = new AsyncStreamReader<PullMessagesRequest>();
        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
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
            .PullMessagesAsync(Arg.Any<PubSubPullMessagesTopic>(), Arg.Any<MessageDeliveryHandler<string?, PubSubPullMessagesResponse>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var reader = new AsyncStreamReader<PullMessagesRequest>();
        var mockWriter = Substitute.For<IServerStreamWriter<PullMessagesResponse>>();

        var pullTask = fixture.Adaptor.PullMessages(
            reader,
            mockWriter,
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
