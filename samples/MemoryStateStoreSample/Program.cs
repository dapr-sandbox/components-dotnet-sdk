using MemoryStateStoreSample.Services;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "memstore",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MemoryStateStore>();
    });

// Use this registration method to have a single state store instance for all components.
// app.AddStateStore<MemoryStateStore>();

// This registration method enables a state store instance per component instance.
//app.AddStateStore(
//    (serviceProvider, instanceId) => 
//    {
//        Console.WriteLine("Creating state store for instance: {0}", instanceId);
//
//        return new MemoryStateStore(serviceProvider.GetRequiredService<ILogger<MemoryStateStore>>());
//    });

app.Run();
