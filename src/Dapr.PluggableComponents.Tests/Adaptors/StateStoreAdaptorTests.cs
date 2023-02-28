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
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class StateStoreAdaptorTests
{
    [Fact]
    public async Task InitTests()
    {
        var mockStateStore = new Mock<IStateStore>();

        mockStateStore
            .Setup(component => component.InitAsync(It.IsAny<Components.MetadataRequest>(), It.IsAny<CancellationToken>()));

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new MetadataRequest();

        metadataRequest.Properties.Add(properties);

        using var context = new TestServerCallContext();

        await adaptor.Init(
            new Proto.Components.V1.InitRequest
            {
                Metadata = metadataRequest
            },
            context);

        mockStateStore
            .Verify(
                component => component.InitAsync(
                    It.Is<Components.MetadataRequest>(request => AssertMetadataEqual(properties, request.Properties)),
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }

    [Fact]
    public async Task SimulatedBulkDelete()
    {
        var mockStateStore = new Mock<IStateStore>(MockBehavior.Strict);

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        using var context = new TestServerCallContext();

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.DeleteAsync(It.Is<StateStoreDeleteRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .Returns(Task.CompletedTask);

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.DeleteAsync(It.Is<StateStoreDeleteRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .Returns(Task.CompletedTask);

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkDeleteRequest = new Proto.Components.V1.BulkDeleteRequest();

        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest{ Key = key1 });
        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest{ Key = key2 });

        await adaptor.BulkDelete(
            bulkDeleteRequest,
            context);

        mockStateStore.VerifyAll();
    }

    [Fact]
    public async Task BulkDelete()
    {
        var mockStateStore = new Mock<IStateStore>();
        var mockBulkStateStore = mockStateStore.As<IBulkStateStore>();

        mockBulkStateStore
            .Setup(component => component.BulkDeleteAsync(It.IsAny<StateStoreDeleteRequest[]>(), It.IsAny<CancellationToken>()));

        string key1 = "key1";
        string key2 = "key2";

        using var context = new TestServerCallContext();

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkDeleteRequest = new Proto.Components.V1.BulkDeleteRequest();

        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest{ Key = key1 });
        bulkDeleteRequest.Items.Add(new Proto.Components.V1.DeleteRequest{ Key = key2 });

        await adaptor.BulkDelete(
            bulkDeleteRequest,
            context);

        mockBulkStateStore
            .Verify(component => component.BulkDeleteAsync(
                It.Is<StateStoreDeleteRequest[]>(request => request.Length == 2 && request[0].Key == key1 && request[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == context.CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task SimulatedBulkSet()
    {
        var mockStateStore = new Mock<IStateStore>(MockBehavior.Strict);

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        using var context = new TestServerCallContext();

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.SetAsync(It.Is<StateStoreSetRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .Returns(Task.CompletedTask);

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.SetAsync(It.Is<StateStoreSetRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .Returns(Task.CompletedTask);

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkSetRequest = new Proto.Components.V1.BulkSetRequest();

        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest{ Key = key1 });
        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest{ Key = key2 });

        await adaptor.BulkSet(
            bulkSetRequest,
            context);

        mockStateStore.VerifyAll();
    }

    [Fact]
    public async Task BulkSet()
    {
        var mockStateStore = new Mock<IStateStore>();
        var mockBulkStateStore = mockStateStore.As<IBulkStateStore>();

        mockBulkStateStore
            .Setup(component => component.BulkSetAsync(It.IsAny<StateStoreSetRequest[]>(), It.IsAny<CancellationToken>()));

        string key1 = "key1";
        string key2 = "key2";

        using var context = new TestServerCallContext();

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkSetRequest = new Proto.Components.V1.BulkSetRequest();

        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest{ Key = key1 });
        bulkSetRequest.Items.Add(new Proto.Components.V1.SetRequest{ Key = key2 });

        await adaptor.BulkSet(
            bulkSetRequest,
            context);

        mockBulkStateStore
            .Verify(component => component.BulkSetAsync(
                It.Is<StateStoreSetRequest[]>(requests => requests.Length == 2 && requests[0].Key == key1 && requests[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == context.CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task SimulatedBulkGet()
    {
        var mockStateStore = new Mock<IStateStore>(MockBehavior.Strict);

        var sequence = new MockSequence();

        string key1 = "key1";
        string key2 = "key2";

        string value1 = "value1";
        string value2 = "value2";

        using var context = new TestServerCallContext();

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.GetAsync(It.Is<StateStoreGetRequest>(request => request.Key == key1), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .ReturnsAsync(new StateStoreGetResponse { Data = Encoding.UTF8.GetBytes(value1) });

        mockStateStore
            .InSequence(sequence)
            .Setup(component => component.GetAsync(It.Is<StateStoreGetRequest>(request => request.Key == key2), It.Is<CancellationToken>(token => token == context.CancellationToken)))
            .ReturnsAsync(new StateStoreGetResponse { Data = Encoding.UTF8.GetBytes(value2) });

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkGetRequest = new Proto.Components.V1.BulkGetRequest();

        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest{ Key = key1 });
        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest{ Key = key2 });

        var response = await adaptor.BulkGet(
            bulkGetRequest,
            context);

        Assert.True(response.Got);
        Assert.Contains(response.Items, item => item.Key == key1 && item.Data.ToStringUtf8() == value1);
        Assert.Contains(response.Items, item => item.Key == key2 && item.Data.ToStringUtf8() == value2);

        mockStateStore.VerifyAll();
    }

    [Fact]
    public async Task BulkGet()
    {
        var mockStateStore = new Mock<IStateStore>(MockBehavior.Strict);
        var mockBulkStateStore = mockStateStore.As<IBulkStateStore>();

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

        using var context = new TestServerCallContext();

        var adaptor = CreateStateStoreAdaptor(mockStateStore.Object);

        var bulkGetRequest = new Proto.Components.V1.BulkGetRequest();

        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest{ Key = key1 });
        bulkGetRequest.Items.Add(new Proto.Components.V1.GetRequest{ Key = key2 });

        var response = await adaptor.BulkGet(
            bulkGetRequest,
            context);

        Assert.True(response.Got);
        Assert.Contains(response.Items, item => item.Key == key1 && item.Data.ToStringUtf8() == value1);
        Assert.Contains(response.Items, item => item.Key == key2 && item.Data.ToStringUtf8() == value2);

        mockBulkStateStore
            .Verify(component => component.BulkGetAsync(It.Is<StateStoreGetRequest[]>(
                requests => requests.Length == 2 && requests[0].Key == key1 && requests[1].Key == key2),
                It.Is<CancellationToken>(cancellationToken => cancellationToken == context.CancellationToken)), Times.Once());
    }

    private static bool AssertMetadataEqual(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected, actual);

        return true;
    }

    private static StateStoreAdaptor CreateStateStoreAdaptor(IStateStore stateStore)
    {
        var logger = new Mock<ILogger<StateStoreAdaptor>>();

        var mockComponentProvider = new Mock<IDaprPluggableComponentProvider<IStateStore>>();

        mockComponentProvider
            .Setup(componentProvider => componentProvider.GetComponent(It.IsAny<ServerCallContext>()))
            .Returns(stateStore);

        return new StateStoreAdaptor(logger.Object, mockComponentProvider.Object);
    }

    private sealed class TestServerCallContext : ServerCallContext, IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        #region ServerCallContext Overrides

        protected override string MethodCore => throw new NotImplementedException();

        protected override string HostCore => throw new NotImplementedException();

        protected override string PeerCore => throw new NotImplementedException();

        protected override DateTime DeadlineCore => throw new NotImplementedException();

        protected override Metadata RequestHeadersCore => throw new NotImplementedException();

        protected override CancellationToken CancellationTokenCore => this.cts.Token;

        protected override Metadata ResponseTrailersCore => throw new NotImplementedException();

        protected override Status StatusCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override AuthContext AuthContextCore => throw new NotImplementedException();

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.cts.Dispose();
        }

        #endregion
    }
}
