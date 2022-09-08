using Dapr.PluggableComponents.AspNetCore;
using DaprInMemoryComponents.Components.StateStore;

namespace DaprInMemoryComponents
{

    class InMemoryPluggableComponent
    {
        static void Main()
        {
            var builder = PluggableComponentServiceBuilder.CreateBuilder();
            var singletonInMemoryStateStore = new InMemoryStateStore();
            builder
                .UseStateStore(() => singletonInMemoryStateStore)
                .Run();
        }
    }

}