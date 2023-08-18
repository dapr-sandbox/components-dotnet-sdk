using Dapr.PluggableComponents;
using LocalEnvSecretStoreSample.Services;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "local.env-pluggable",
    serviceBuilder =>
    {
        serviceBuilder.RegisterSecretStore(
            context =>
            {
                Console.WriteLine("Creating secret store for instance '{0}' on socket '{1}'...", context.InstanceId, context.SocketPath);
                return new LocalEnvSecretStore(context.ServiceProvider.GetRequiredService<ILogger<LocalEnvSecretStore>>());
            });
    });

app.Run();