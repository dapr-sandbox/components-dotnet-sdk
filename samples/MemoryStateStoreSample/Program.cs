using MemoryStateStoreSample.Services;

var componentName = "memstore";

var options = new DaprPluggableComponentsOptions
    {
        Args = args,
        SocketName = componentName
    };

var app = DaprPluggableComponentsApplication.Create(options);

// app.AddStateStore<MemoryStateStore>();

app.AddStateStore(
    instanceId => 
    {
        Console.WriteLine("Creating state store for instance: {0}", instanceId);

        return new MemoryStateStore();
    });

app.Run();
