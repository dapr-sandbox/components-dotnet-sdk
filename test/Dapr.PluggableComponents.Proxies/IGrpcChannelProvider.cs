using Grpc.Net.Client;

namespace Dapr.PluggableComponents.Proxies;

internal interface IGrpcChannelProvider
{
    GrpcChannel GetChannel();
}