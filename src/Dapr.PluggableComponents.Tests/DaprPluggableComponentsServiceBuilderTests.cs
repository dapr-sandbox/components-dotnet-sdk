﻿// ------------------------------------------------------------------------
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

using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static Dapr.Proto.Components.V1.QueriableStateStore;
using static Dapr.Proto.Components.V1.StateStore;
using static Dapr.Proto.Components.V1.TransactionalStateStore;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsServiceBuilderTests
{
    [Fact]
    public async Task RegisterSingletonStateStore()
    {
        var mockStateStore = new Mock<IMockStateStore<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore.Object);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockStateStore<Unit>>();
            });

        await application.StartAsync();

        var client = new StateStoreClient(socketFixture.GrpcChannel);

        var metadataA = new Grpc.Core.Metadata { new Grpc.Core.Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "A") };
        var metadataB = new Grpc.Core.Metadata { new Grpc.Core.Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "B") };

        await client.InitAsync(new Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);
        await client.InitAsync(new Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataB);

        mockStateStore.Verify(stateStore => stateStore.Create(), Times.Once());
        mockStateStore.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterFactoryBasedStateStore()
    {
        var mockStateStoreA = new Mock<IMockStateStore<Unit>>();
        var mockStateStoreB = new Mock<IMockStateStore<Unit>>();

        const string componentInstanceA = "A";
        const string componentInstanceB = "B";

        using var application = DaprPluggableComponentsApplication.Create();

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore(
                    context =>
                    {
                        return context.InstanceId switch
                        {
                            componentInstanceA => new MockStateStore<Unit>(mockStateStoreA.Object),
                            componentInstanceB => new MockStateStore<Unit>(mockStateStoreB.Object),
                            _ => throw new Exception()
                        };
                    });
            });

        await application.StartAsync();

        var client = new StateStoreClient(socketFixture.GrpcChannel);

        var metadataA = new Grpc.Core.Metadata { new Grpc.Core.Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, componentInstanceA) };
        var metadataB = new Grpc.Core.Metadata { new Grpc.Core.Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, componentInstanceB) };

        await client.InitAsync(new Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);
        await client.InitAsync(new Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);
        await client.InitAsync(new Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataB);

        mockStateStoreA.Verify(stateStore => stateStore.Create(), Times.Once());
        mockStateStoreB.Verify(stateStore => stateStore.Create(), Times.Once());

        mockStateStoreA.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockStateStoreB.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RegisterBulkStateStore()
    {
        var mockStateStore = new Mock<IMockBulkStateStore<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore.Object);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockBulkStateStore<Unit>>();
            });

        await application.StartAsync();

        var client = new StateStoreClient(socketFixture.GrpcChannel);

        await client.BulkSetAsync(new Proto.Components.V1.BulkSetRequest());

        mockStateStore.Verify(stateStore => stateStore.BulkSetAsync(It.IsAny<StateStoreSetRequest[]>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RegisterTransactionalStateStore()
    {
        var mockStateStore = new Mock<IMockTransactionalStateStore<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore.Object);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockTransactionalStateStore<Unit>>();
            });

        await application.StartAsync();

        var client = new TransactionalStateStoreClient(socketFixture.GrpcChannel);

        await client.TransactAsync(new Proto.Components.V1.TransactionalStateRequest());

        mockStateStore.Verify(stateStore => stateStore.TransactAsync(It.IsAny<StateStoreTransactRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RegisterQueryableStateStore()
    {
        var mockStateStore = new Mock<IMockQueryableStateStore<Unit>>();

        mockStateStore
            .Setup(stateStore => stateStore.QueryAsync(It.IsAny<StateStoreQueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StateStoreQueryResponse());

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore.Object);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockQueryableStateStore<Unit>>();
            });

        await application.StartAsync();

        var client = new QueriableStateStoreClient(socketFixture.GrpcChannel);

        await client.QueryAsync(new Proto.Components.V1.QueryRequest());

        mockStateStore.Verify(stateStore => stateStore.QueryAsync(It.IsAny<StateStoreQueryRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task RegisterSimilarComponentsAcrossServices()
    {
        var mockStateStoreA = new Mock<IMockStateStore<Unit.A>>();
        var mockStateStoreB = new Mock<IMockStateStore<Unit.B>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStoreA.Object);
        application.Services.AddSingleton(_ => mockStateStoreB.Object);

        using var socketFixtureA = new SocketFixture();
        using var socketFixtureB = new SocketFixture();

        application.RegisterService(
            socketFixtureA.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockStateStore<Unit.A>>();
            });

        application.RegisterService(
            socketFixtureB.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockStateStore<Unit.B>>();
            });

        await application.StartAsync();

        var clientA = new StateStoreClient(socketFixtureA.GrpcChannel);
        var clientB = new StateStoreClient(socketFixtureB.GrpcChannel);

        await clientA.InitAsync(new Dapr.Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() });

        mockStateStoreA.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        mockStateStoreB.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Never());

        await clientB.InitAsync(new Dapr.Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() });

        mockStateStoreA.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        mockStateStoreB.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
