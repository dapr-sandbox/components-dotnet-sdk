using MemoryStateStoreSample.Services;

var componentName = "memstore";

var options = new DaprPluggableComponentsOptions
    {
        Args = args,
        SocketName = componentName
    };

var app = DaprPluggableComponentsApplication.Create(options);

// app.UseStateStore<MemoryStateStore>();

app.UseStateStore(
    instanceId => 
    {
        Console.WriteLine("Creating state store for instance: {0}", instanceId);

        return new MemoryStateStore();
    });

app.Run();
