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

using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.PluggableComponents.Components.StateStore;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using static Dapr.Proto.Components.V1.QueriableStateStore;
using static Dapr.Proto.Components.V1.StateStore;
using static Dapr.Proto.Components.V1.TransactionalStateStore;

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsServiceBuilderTests
{
    private static async Task TestSingletonInitAsync<TMockInterface, TClient>(
        Func<GrpcChannel, TClient> clientFactory,
        Action<DaprPluggableComponentsServiceBuilder> registerCall,
        Func<TClient, Grpc.Core.Metadata, Task> initCall)
        where TMockInterface : class, IMockPluggableComponent
    {
        var mockInputBinding = Substitute.For<TMockInterface>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockInputBinding);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                registerCall(serviceBuilder);
            });

        await application.StartAsync();

        var client = clientFactory(socketFixture.GrpcChannel);

        var metadataA = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "A") };
        var metadataB = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "B") };

        await initCall(client, metadataA);
        await initCall(client, metadataB);

        mockInputBinding.Received(1).Create();
        await mockInputBinding.Received(2).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
    }

    private static async Task TestFactoryBasedInitAsync<TMockInterface, TMockType, TClient>(
        Func<GrpcChannel, TClient> clientFactory,
        Func<TMockInterface, TMockType> componentFactory,
        Func<DaprPluggableComponentsServiceBuilder, Action<ComponentProviderDelegate<TMockType>>> registerCall,
        Func<TClient, Grpc.Core.Metadata, Task> initCall)
        where TMockInterface : class, IMockPluggableComponent
        where TMockType : class
    {
        var mockComponentA = Substitute.For<TMockInterface>();
        var mockComponentB = Substitute.For<TMockInterface>();

        const string componentInstanceA = "A";
        const string componentInstanceB = "B";

        using var application = DaprPluggableComponentsApplication.Create();

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                registerCall(serviceBuilder)(
                    context =>
                    {
                        return context.InstanceId switch
                        {
                            componentInstanceA => componentFactory(mockComponentA),
                            componentInstanceB => componentFactory(mockComponentB),
                            _ => throw new Exception()
                        };
                    });
            });

        await application.StartAsync();

        var client = clientFactory(socketFixture.GrpcChannel);

        var metadataA = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, componentInstanceA) };
        var metadataB = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, componentInstanceB) };

        await initCall(client, metadataA);
        await initCall(client, metadataA);
        await initCall(client, metadataB);

        mockComponentA.Received(1).Create();
        mockComponentB.Received(1).Create();

        await mockComponentA.Received(2).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
        await mockComponentB.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public Task RegisterSingletonInputBinding()
    {
        return TestSingletonInitAsync<IMockInputBinding<Unit>, InputBinding.InputBindingClient>(
            channel => new InputBinding.InputBindingClient(channel),
            serviceBuilder => serviceBuilder.RegisterBinding<MockInputBinding<Unit>>(),
            async (client, metadata) => await client.InitAsync(new InputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterFactoryBasedInputBinding()
    {
        return TestFactoryBasedInitAsync<IMockInputBinding<Unit>, MockInputBinding<Unit>, InputBinding.InputBindingClient>(
            channel => new InputBinding.InputBindingClient(channel),
            component => new MockInputBinding<Unit>(component),
            serviceBuilder => factory => serviceBuilder.RegisterBinding<MockInputBinding<Unit>>(factory),
            async (client, metadata) => await client.InitAsync(new InputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterSingletonOutputBinding()
    {
        return TestSingletonInitAsync<IMockOutputBinding<Unit>, OutputBinding.OutputBindingClient>(
            channel => new OutputBinding.OutputBindingClient(channel),
            serviceBuilder => serviceBuilder.RegisterBinding<MockOutputBinding<Unit>>(),
            async (client, metadata) => await client.InitAsync(new OutputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterFactoryBasedOutputBinding()
    {
        return TestFactoryBasedInitAsync<IMockOutputBinding<Unit>, MockOutputBinding<Unit>, OutputBinding.OutputBindingClient>(
            channel => new OutputBinding.OutputBindingClient(channel),
            component => new MockOutputBinding<Unit>(component),
            serviceBuilder => factory => serviceBuilder.RegisterBinding<MockOutputBinding<Unit>>(factory),
            async (client, metadata) => await client.InitAsync(new OutputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public async Task RegisterSeparateInputOutputBindings()
    {
        var mockInputBinding = Substitute.For<IMockInputBinding<Unit>>();
        var mockOutputBinding = Substitute.For<IMockOutputBinding<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockInputBinding);
        application.Services.AddSingleton(_ => mockOutputBinding);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterBinding<MockInputBinding<Unit>>();
                serviceBuilder.RegisterBinding<MockOutputBinding<Unit>>();
            });

        await application.StartAsync();

        var inputClient = new InputBinding.InputBindingClient(socketFixture.GrpcChannel);
        var outputClient = new OutputBinding.OutputBindingClient(socketFixture.GrpcChannel);

        var metadataA = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "A") };

        await inputClient.InitAsync(new InputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);
        await outputClient.InitAsync(new OutputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);

        mockInputBinding.Received(1).Create();
        await mockInputBinding.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());

        mockInputBinding.Received(1).Create();
        await mockInputBinding.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterCombinedInputOutputBindings()
    {
        var mockCombinedBinding = Substitute.For<IMockCombinedBinding<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockCombinedBinding);

        using var socketFixture = new SocketFixture();

        application.RegisterService(
            socketFixture.ServiceOptions,
            serviceBuilder =>
            {
                serviceBuilder.RegisterBinding<MockCombinedBinding<Unit>>();
            });

        await application.StartAsync();

        var inputClient = new InputBinding.InputBindingClient(socketFixture.GrpcChannel);
        var outputClient = new OutputBinding.OutputBindingClient(socketFixture.GrpcChannel);

        var metadataA = new Metadata { new Metadata.Entry(TestConstants.Metadata.ComponentInstanceId, "A") };

        await inputClient.InitAsync(new InputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);
        await outputClient.InitAsync(new OutputBindingInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadataA);

        mockCombinedBinding.Received(1).Create();
        await mockCombinedBinding.Received(2).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public Task RegisterSingletonPubSub()
    {
        return TestSingletonInitAsync<IMockPubSub<Unit>, PubSub.PubSubClient>(
            channel => new PubSub.PubSubClient(channel),
            serviceBuilder => serviceBuilder.RegisterPubSub<MockPubSub<Unit>>(),
            async (client, metadata) => await client.InitAsync(new Proto.Components.V1.PubSubInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterFactoryBasedPubSub()
    {
        return TestFactoryBasedInitAsync<IMockPubSub<Unit>, MockPubSub<Unit>, PubSub.PubSubClient>(
            channel => new PubSub.PubSubClient(channel),
            component => new MockPubSub<Unit>(component),
            serviceBuilder => factory => serviceBuilder.RegisterPubSub<MockPubSub<Unit>>(factory),
            async (client, metadata) => await client.InitAsync(new Proto.Components.V1.PubSubInitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterSingletonStateStore()
    {
        return TestSingletonInitAsync<IMockStateStore<Unit>, StateStore.StateStoreClient>(
            channel => new StateStore.StateStoreClient(channel),
            serviceBuilder => serviceBuilder.RegisterStateStore<MockStateStore<Unit>>(),
            async (client, metadata) => await client.InitAsync(new InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public Task RegisterFactoryBasedStateStore()
    {
        return TestFactoryBasedInitAsync<IMockStateStore<Unit>, MockStateStore<Unit>, StateStore.StateStoreClient>(
            channel => new StateStore.StateStoreClient(channel),
            component => new MockStateStore<Unit>(component),
            serviceBuilder => factory => serviceBuilder.RegisterStateStore<MockStateStore<Unit>>(factory),
            async (client, metadata) => await client.InitAsync(new InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() }, metadata));
    }

    [Fact]
    public async Task RegisterBulkStateStore()
    {
        var mockStateStore = Substitute.For<IMockBulkStateStore<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore);

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

        await mockStateStore.Received(1).BulkSetAsync(Arg.Any<StateStoreSetRequest[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterTransactionalStateStore()
    {
        var mockStateStore = Substitute.For<IMockTransactionalStateStore<Unit>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore);

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

        await mockStateStore.Received(1).TransactAsync(Arg.Any<StateStoreTransactRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterQueryableStateStore()
    {
        var mockStateStore = Substitute.For<IMockQueryableStateStore<Unit>>();

        mockStateStore
            .QueryAsync(Arg.Any<StateStoreQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new StateStoreQueryResponse());

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStore);

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

        await mockStateStore.Received(1).QueryAsync(Arg.Any<StateStoreQueryRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterSimilarComponentsAcrossServices()
    {
        var mockStateStoreA = Substitute.For<IMockStateStore<Unit.A>>();
        var mockStateStoreB = Substitute.For<IMockStateStore<Unit.B>>();

        using var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton(_ => mockStateStoreA);
        application.Services.AddSingleton(_ => mockStateStoreB);

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

        await mockStateStoreA.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
        await mockStateStoreB.DidNotReceive().InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());

        await clientB.InitAsync(new Dapr.Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() });

        await mockStateStoreA.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
        await mockStateStoreB.Received(1).InitAsync(Arg.Any<MetadataRequest>(), Arg.Any<CancellationToken>());
    }
}
