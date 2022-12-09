using MemoryStateStoreSample.Services;

var app = DaprPluggableComponentsApplication.Create("memstore");

// app.AddStateStore<MemoryStateStore>();

app.AddStateStore(
    (_, instanceId) => 
    {
        Console.WriteLine("Creating state store for instance: {0}", instanceId);

        return new MemoryStateStore();
    });

app.Run();
