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

using System.Text;
using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components.StateStore;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class StateStoreAdaptorTests
{
    [Fact]
    public Task InitTests()
    {
        return AdaptorFixture.TestInitAsync(
            () => AdaptorFixture.CreateStateStore(),
            (fixture, metadataRequest) => fixture.Adaptor.Init(new Proto.Components.V1.InitRequest { Metadata = metadataRequest }, fixture.Context));
    }

    [Fact]
    public Task PingTests()
    {
        return AdaptorFixture.TestPingAsync<StateStoreAdaptor, IStateStore>(
            AdaptorFixture.CreateStateStore,
            fixture => fixture.Adaptor.Ping(new PingRequest(), fixture.Context));
    }

    [Fact]
    public async Task SimulatedBulkDelete()
    {
        using var fixture = AdaptorFixture.CreateStateStore(new Mock<IStateStore>(MockBehavior.Strict));

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.DeleteAsync(It.Is<StateStoreDeleteRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .Returns(Task.CompletedTask);

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.DeleteAsync(It.Is<StateStoreDeleteRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .Returns(Task.CompletedTask);

        var bulkDeleteRequest = new Proto.Components.V1.BulkDeleteRequest();

        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest { Key = key1 });
        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest { Key = key2 });

        await fixture.Adaptor.BulkDelete(
            bulkDeleteRequest,
            fixture.Context);

        fixture.MockComponent.VerifyAll();
    }

    [Fact]
    public async Task BulkDelete()
    {
        using var fixture = AdaptorFixture.CreateStateStore();

        var mockBulkStateStore = fixture.MockComponent.As<IBulkStateStore>();

        mockBulkStateStore
            .Setup(component => component.BulkDeleteAsync(It.IsAny<StateStoreDeleteRequest[]>(), It.IsAny<CancellationToken>()));

        string key1 = "key1";
        string key2 = "key2";

        var bulkDeleteRequest = new Proto.Components.V1.BulkDeleteRequest();

        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest { Key = key1 });
        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest { Key = key2 });

        await fixture.Adaptor.BulkDelete(
            bulkDeleteRequest,
            fixture.Context);

        mockBulkStateStore
            .Verify(component => component.BulkDeleteAsync(
                It.Is<StateStoreDeleteRequest[]>(request => request.Length == 2 && request[0].Key == key1 && request[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == fixture.Context.CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task SimulatedBulkSet()
    {
        using var fixture = AdaptorFixture.CreateStateStore(new Mock<IStateStore>(MockBehavior.Strict));

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.SetAsync(It.Is<StateStoreSetRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .Returns(Task.CompletedTask);

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.SetAsync(It.Is<StateStoreSetRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .Returns(Task.CompletedTask);

        var bulkSetRequest = new Proto.Components.V1.BulkSetRequest();

        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest { Key = key1 });
        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest { Key = key2 });

        await fixture.Adaptor.BulkSet(
            bulkSetRequest,
            fixture.Context);

        fixture.MockComponent.VerifyAll();
    }

    [Fact]
    public async Task BulkSet()
    {
        using var fixture = AdaptorFixture.CreateStateStore();

        var mockBulkStateStore = fixture.MockComponent.As<IBulkStateStore>();

        mockBulkStateStore
            .Setup(component => component.BulkSetAsync(It.IsAny<StateStoreSetRequest[]>(), It.IsAny<CancellationToken>()));

        string key1 = "key1";
        string key2 = "key2";

        var bulkSetRequest = new Proto.Components.V1.BulkSetRequest();

        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest { Key = key1 });
        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest { Key = key2 });

        await fixture.Adaptor.BulkSet(
            bulkSetRequest,
            fixture.Context);

        mockBulkStateStore
            .Verify(component => component.BulkSetAsync(
                It.Is<StateStoreSetRequest[]>(requests => requests.Length == 2 && requests[0].Key == key1 && requests[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == fixture.Context.CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task SimulatedBulkGet()
    {
        using var fixture = AdaptorFixture.CreateStateStore(new Mock<IStateStore>(MockBehavior.Strict));

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        string value1 = "value1";
        string value2 = "value2";

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.GetAsync(It.Is<StateStoreGetRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .ReturnsAsync(new StateStoreGetResponse { Data = Encoding.UTF8.GetBytes(value1) });

        fixture.MockComponent
            .InSequence(sequence)
            .Setup(component => component.GetAsync(It.Is<StateStoreGetRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)))
            .ReturnsAsync(new StateStoreGetResponse { Data = Encoding.UTF8.GetBytes(value2) });

        var bulkGetRequest = new Proto.Components.V1.BulkGetRequest();

        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest { Key = key1 });
        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest { Key = key2 });

        var response = await fixture.Adaptor.BulkGet(
            bulkGetRequest,
            fixture.Context);

        Assert.Contains(response.Items, item => item.Key == key1 && item.Data.ToStringUtf8() == value1);
        Assert.Contains(response.Items, item => item.Key == key2 && item.Data.ToStringUtf8() == value2);

        fixture.MockComponent.VerifyAll();
    }

    [Fact]
    public async Task BulkGet()
    {
        using var fixture = AdaptorFixture.CreateStateStore(new Mock<IStateStore>(MockBehavior.Strict));

        var mockBulkStateStore = fixture.MockComponent.As<IBulkStateStore>();

        string key1 = "key1";
        string key2 = "key2";

        string value1 = "value1";
        string value2 = "value2";

        mockBulkStateStore
            .Setup(component => component.BulkGetAsync(It.IsAny<StateStoreGetRequest[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StateStoreBulkStateItem[]
                {
                    new StateStoreBulkStateItem(key1) { Data = Encoding.UTF8.GetBytes(value1) },
                    new StateStoreBulkStateItem(key2) { Data = Encoding.UTF8.GetBytes(value2) }
                });

        var bulkGetRequest = new Proto.Components.V1.BulkGetRequest();

        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest { Key = key1 });
        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest { Key = key2 });

        var response = await fixture.Adaptor.BulkGet(
            bulkGetRequest,
            fixture.Context);

        Assert.Contains(response.Items, item => item.Key == key1 && item.Data.ToStringUtf8() == value1);
        Assert.Contains(response.Items, item => item.Key == key2 && item.Data.ToStringUtf8() == value2);

        mockBulkStateStore
            .Verify(component => component.BulkGetAsync(It.Is<StateStoreGetRequest[]>(
                requests => requests.Length == 2 && requests[0].Key == key1 && requests[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == fixture.Context.CancellationToken)), Times.Once());
    }
}
