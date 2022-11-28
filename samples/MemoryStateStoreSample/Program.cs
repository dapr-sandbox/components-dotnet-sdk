using MemoryStateStoreSample.Services;

var componentName = "memstore";

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var options = new DaprPluggableComponentsOptions
    {
        Args = args,
        SocketName = componentName
    };

var app = DaprPluggableComponentsApplication.Create(options);

app.UseStateStore<MemoryStateStore>();

app.Run();
