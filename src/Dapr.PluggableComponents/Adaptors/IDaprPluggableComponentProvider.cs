using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public interface IDaprPluggableComponentProvider<out T>
{
    T GetComponent(ServerCallContext context);
}
