using System.Net.Sockets;
using Grpc.Net.Client;

namespace Dapr.PluggableComponents.Proxies;

internal sealed class SocketBasedGrpcChannelProvider : IGrpcChannelProvider, IDisposable
{
    private readonly Lazy<GrpcChannel> channelFactory;

    public SocketBasedGrpcChannelProvider(string socketPath)
    {
        this.channelFactory = new Lazy<GrpcChannel>(
            () =>
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
                                            await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketPath), cancellationToken).ConfigureAwait(false);
                                            
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
            });
    }

    #region IGrpcChannelProvider Members

    public GrpcChannel GetChannel()
    {
        return this.channelFactory.Value;
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        if (this.channelFactory.IsValueCreated)
        {
            this.channelFactory.Value.Dispose();
        }
    }

    #endregion
}
