using Dapr.PluggableComponents;
using Dapr.PluggableComponents.Proxies;
using Dapr.PluggableComponents.Proxies.Components;

const string SocketsPathEnvironmentVariable = "DAPR_COMPONENTS_PROXIES_SOCKET_PATH";

string? socketPath = Environment.GetEnvironmentVariable(SocketsPathEnvironmentVariable);

if (String.IsNullOrEmpty(socketPath))
{
    throw new InvalidOperationException($"The environment variable {SocketsPathEnvironmentVariable} must be set.");
}

var options = new DaprPluggableComponentsApplicationOptions
{
    WebApplicationBuilderConfiguration =
        builder =>
        {
            builder.Services.AddSingleton<IGrpcChannelProvider>(_ => new SocketBasedGrpcChannelProvider(socketPath));
        }
};

var app = DaprPluggableComponentsApplication.Create(options);

app.RegisterService(
    "proxies",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<ProxyStateStore>();
    });

app.Run();
