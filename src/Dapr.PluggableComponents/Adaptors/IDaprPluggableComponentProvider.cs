using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public interface IDaprPluggableComponentProvider<T>
{
    T GetComponent(ServerCallContext context);
}
