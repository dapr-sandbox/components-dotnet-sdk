using MemoryStateStoreSample.Services;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "memstore",
    serviceBuilder =>
    {
        // Use this registration method to have a single state store instance for all components.
        // serviceBuilder.RegisterStateStore<MemoryStateStore>();

        // This registration method enables a state store instance per component instance.
        serviceBuilder.RegisterStateStore(
            (serviceProvider, instanceId) => 
            {
                Console.WriteLine("Creating state store for instance: {0}", instanceId);

                return new MemoryStateStore(serviceProvider.GetRequiredService<ILogger<MemoryStateStore>>());
            });
    });

app.Run();
