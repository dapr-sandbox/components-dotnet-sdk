using Dapr.Client;
using Dapr.PluggableComponents;
using ProxyComponentsSample.Components;

var options = new DaprPluggableComponentsApplicationOptions
{
    SocketName = "proxy",
    WebApplicationBuilderConfiguration =
        builder =>
        {
            builder.Services.AddDaprClient();
        }
};

var app = DaprPluggableComponentsApplication.Create(options);

app.AddStateStore(
    (serviceProvider, instanceId) => new ProxyStateStore(
        instanceId,
        serviceProvider.GetService<DaprClient>(),
        serviceProvider.GetService<ILogger<ProxyStateStore>>()));

app.Run();
