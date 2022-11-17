var componentName = "memstore";

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var builder = DaprPluggableComponentsApplication.CreateBuilder(args, new DaprPluggableComponentOptions { SocketName = componentName });

builder.Services.AddSingleton<IStateStore, MemoryStateStore.Services.MemoryStateStore>();

var app = builder.Build();

app.MapDaprPluggableComponents();

// register our memstore
//app.UseDaprPluggableComponent<MemStoreService>();

app.UseDaprPluggableComponent<StateStoreAdaptor>();

app.Run();
