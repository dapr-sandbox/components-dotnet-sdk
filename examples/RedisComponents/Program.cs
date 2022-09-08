using Dapr.PluggableComponents.AspNetCore;
using DaprRedisComponents.Components.StateStore;

namespace DaprRedisComponents
{

    class InMemoryPluggableComponent
    {
        static void Main()
        {
            var service = PluggableComponentServiceBuilder.CreateBuilder();
            service.UseStateStore(() => new RedisStateStore());
            service.Run();
        }
    }
}