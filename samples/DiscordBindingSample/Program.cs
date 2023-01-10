using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "discord-binding",
    serviceBuilder =>
    {
        // Use this registration method to have a single binding instance for all components.
        // serviceBuilder.RegisterBinding<DiscordBinding>();

        // This registration method enables a binding instance per component instance.
        serviceBuilder.RegisterBinding(
            context =>
            {
                Console.WriteLine("Creating binding for instance '{0}' on socket '{1}'...", context.InstanceId, context.SocketPath);

                return new DiscordBinding(context.ServiceProvider.GetRequiredService<ILogger<DiscordBinding>>());
            });
    });

app.Run();
