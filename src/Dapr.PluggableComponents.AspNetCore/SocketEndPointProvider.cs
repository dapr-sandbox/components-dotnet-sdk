using Dapr.PluggableComponents.Adaptors;
using Grpc.Core;
using Microsoft.AspNetCore.Connections.Features;

namespace Dapr.PluggableComponents;

internal sealed class SocketEndPointProvider : IDaprPluggableComponentEndPointProvider
{
    #region IDaprPluggableComponentEndPointProvider Members

    public string GetEndPoint(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var socketFeature = httpContext.Features.Get<IConnectionSocketFeature>();        
        var socketPath = socketFeature?.Socket.LocalEndPoint?.ToString();

        if (socketPath == null)
        {
            throw new InvalidOperationException("Unable to determine the socket on which a gRPC request was made.");
        }

        return socketPath;
    }

    #endregion
}