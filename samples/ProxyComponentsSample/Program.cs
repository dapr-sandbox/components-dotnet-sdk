using Dapr.PluggableComponents;
using ProxyComponentsSample.Components;

var app = DaprPluggableComponentsApplication.Create("proxy");

app.AddStateStore((serviceProvider, instanceId) => new ProxyStateStore(instanceId, serviceProvider.GetService<ILogger<ProxyStateStore>>()));

app.Run();
