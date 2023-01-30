using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "azure-storage-queues",
    serviceBuilder =>
    {
        // Use this registration method to have a single pub-sub instance for all components.
        // serviceBuilder.RegisterPubSub<AzureStorageQueuesPubSub>();

        // This registration method enables a pub-sub instance per component instance.
        serviceBuilder.RegisterPubSub(
            context =>
            {
                Console.WriteLine("Creating pub-sub for instance '{0}' on socket '{1}'...", context.InstanceId, context.SocketPath);

                return new AzureStorageQueuesPubSub(context.ServiceProvider.GetRequiredService<ILogger<AzureStorageQueuesPubSub>>());
            });
    });

app.Run();
