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
using Grpc.Net.Client;

namespace Dapr.PluggableComponents;

internal sealed class SocketFixture : IDisposable
{
    private readonly Lazy<GrpcChannel> grpcChannel;
    private readonly Lazy<DaprPluggableComponentsServiceOptions> serviceOptions;

    public SocketFixture()
    {
        this.grpcChannel = new Lazy<GrpcChannel>(
            () => GrpcChannel.ForAddress(
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
                                        await socket.ConnectAsync(new UnixDomainSocketEndPoint(this.SocketPath), cancellationToken);

                                        return new NetworkStream(socket, true);
                                    }
                                    catch
                                    {
                                        socket.Dispose();

                                        throw;
                                    }
                                }
                        }
                }));

        this.serviceOptions = new Lazy<DaprPluggableComponentsServiceOptions>(
            () => new DaprPluggableComponentsServiceOptions(Path.GetFileNameWithoutExtension(this.SocketPath))
            {
                SocketExtension = Path.GetExtension(this.SocketPath),
                SocketFolder = Path.GetDirectoryName(this.SocketPath)
            });
    }

    public GrpcChannel GrpcChannel => this.grpcChannel.Value;

    public DaprPluggableComponentsServiceOptions ServiceOptions => this.serviceOptions.Value;

    private string SocketPath { get; } = Path.GetTempFileName();

    #region IDisposable Members

    public void Dispose()
    {
        if (this.grpcChannel.IsValueCreated)
        {
            this.grpcChannel.Value.Dispose();
        }

        if (File.Exists(this.SocketPath))
        {
            File.Delete(this.SocketPath);
        }
    }

    #endregion
}
