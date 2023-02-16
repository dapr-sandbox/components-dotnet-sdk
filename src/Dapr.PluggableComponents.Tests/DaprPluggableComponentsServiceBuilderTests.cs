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

using System.Net.Sockets;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static Dapr.Proto.Components.V1.StateStore;

namespace Dapr.PluggableComponents;

internal interface IMockStateStore : IStateStore
{
}

internal sealed class MockStateStore : IMockStateStore
{
    private readonly IMockStateStore proxy;

    public MockStateStore(IMockStateStore proxy)
    {
        this.proxy = proxy;
    }

    #region IStateStore Members

    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.DeleteAsync(request, cancellationToken);
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.GetAsync(request, cancellationToken);
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.InitAsync(request, cancellationToken);
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.SetAsync(request, cancellationToken);
    }

    #endregion
}

public sealed class DaprPluggableComponentsServiceBuilderTests : IDisposable
{
    #region Test Setup and Teardown

    private readonly string socketPath;

    public DaprPluggableComponentsServiceBuilderTests()
    {
        this.socketPath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(this.socketPath))
        {
            File.Delete(this.socketPath);
        }
    }

    #endregion

    [Fact]
    public async Task RegisterSingletonStateStore()
    {
        var mockStateStore = new Mock<IMockStateStore>();

        var application = DaprPluggableComponentsApplication.Create();

        application.Services.AddSingleton<IMockStateStore>(_ => mockStateStore.Object);

        application.RegisterService(
            CreateServiceOptionsForSocket(this.socketPath),
            serviceBuilder =>
            {
                serviceBuilder.RegisterStateStore<MockStateStore>();
            });

        await application.StartAsync();

        var grpcChannel = CreateGrpcChannelForSocket(this.socketPath);

        var client = new StateStoreClient(grpcChannel);

        await client.InitAsync(new Dapr.Proto.Components.V1.InitRequest { Metadata = new Client.Autogen.Grpc.v1.MetadataRequest() });

        await application.StopAsync();

        mockStateStore.Verify(stateStore => stateStore.InitAsync(It.IsAny<MetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    private static DaprPluggableComponentsServiceOptions CreateServiceOptionsForSocket(string socketPath)
    {
        return new DaprPluggableComponentsServiceOptions(Path.GetFileNameWithoutExtension(socketPath))
        {
            SocketExtension = Path.GetExtension(socketPath),
            SocketFolder = Path.GetDirectoryName(socketPath)
        };
    }

    private static GrpcChannel CreateGrpcChannelForSocket(string socketPath)
    {
        return GrpcChannel.ForAddress(
            "http://localhost",
            new GrpcChannelOptions
            {
                HttpHandler =
                    new SocketsHttpHandler
                    {
                        ConnectCallback =
                            async (_, cancellationToken) =>
                            {
                                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                                try
                                {
                                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketPath), cancellationToken);

                                    return new NetworkStream(socket, true);
                                }
                                catch
                                {
                                    socket.Dispose();

                                    throw;
                                }
                            }
                    }
            });
    }
}
