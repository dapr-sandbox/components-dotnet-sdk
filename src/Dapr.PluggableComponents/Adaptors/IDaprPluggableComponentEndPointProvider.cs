using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public interface IDaprPluggableComponentEndPointProvider
{
    string GetEndPoint(ServerCallContext context);
}
