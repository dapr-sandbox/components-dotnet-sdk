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
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class InputBindingAdaptorTests
{
    private const int TimeoutInMs = 10000;

    [Fact]
    public Task InitTests()
    {
        return AdaptorFixture.TestInitAsync(
            () => AdaptorFixture.CreateInputBinding(),
            (fixture, metadataRequest) => fixture.Adaptor.Init(new Proto.Components.V1.InputBindingInitRequest { Metadata = metadataRequest }, fixture.Context));
    }

    [Fact]
    public Task PingTests()
    {
        return AdaptorFixture.TestPingAsync<InputBindingAdaptor, IInputBinding>(
            AdaptorFixture.CreateInputBinding,
            fixture => fixture.Adaptor.Ping(new PingRequest(), fixture.Context));
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task ReadNoMessages()
    {
        using var fixture = AdaptorFixture.CreateInputBinding();

        var reader = new AsyncStreamReader<ReadRequest>();

        fixture.MockComponent
            .Setup(component => component.ReadAsync(It.IsAny<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>>(), It.IsAny<CancellationToken>()))
            .Returns<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>, CancellationToken>(
                (deliveryHandler, cancellationToken) =>
                {
                    return Task.CompletedTask;
                });

        var mockWriter = new Mock<IServerStreamWriter<ReadResponse>>();

        await fixture.Adaptor.Read(
            reader,
            mockWriter.Object,
            fixture.Context);

        fixture.MockComponent
            .Verify(
                component => component.ReadAsync(
                    It.IsAny<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>>(),
                    // NOTE: The adaptor provides its own cancellation token.
                    It.IsAny<CancellationToken>()),
                Times.Once());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task ReadWithMessage()
    {
        using var fixture = AdaptorFixture.CreateInputBinding();

        string contentType = "application/json";
        string error = "error";

        var reader = new AsyncStreamReader<ReadRequest>();

        fixture.MockComponent
            .Setup(component => component.ReadAsync(It.IsAny<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>>(), It.IsAny<CancellationToken>()))
            .Returns<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>, CancellationToken>(
                async (deliveryHandler, cancellationToken) =>
                {
                    await deliveryHandler(
                        new InputBindingReadResponse { ContentType = contentType },
                        response =>
                        {
                            Assert.Equal(error, response.ResponseErrorMessage);

                            reader.Complete();

                            return Task.CompletedTask;
                        });
                });

        var mockWriter = new Mock<IServerStreamWriter<ReadResponse>>();

        mockWriter
            .Setup(writer => writer.WriteAsync(It.IsAny<ReadResponse>()))
            .Returns<ReadResponse>(
                async response =>
                {
                    Assert.Equal(contentType, response.ContentType);

                    await reader.AddAsync(new ReadRequest { ResponseError = new AckResponseError { Message = error }, MessageId = response.MessageId });
                });

        var pullMessagesTask = fixture.Adaptor.Read(
            reader,
            mockWriter.Object,
            fixture.Context);

        await pullMessagesTask;

        fixture.MockComponent
            .Verify(
                component => component.ReadAsync(
                    It.IsAny<MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse>>(),
                    // NOTE: The adaptor provides its own cancellation token.
                    It.IsAny<CancellationToken>()),
                Times.Once());
    }
}
