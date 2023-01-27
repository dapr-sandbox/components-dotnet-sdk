---
type: docs
title: "Application Environment of a .NET Dapr Pluggable Component"
linkTitle: "Application environment"
weight: 1000
description: How to configure the environment of a .NET Pluggable Component
no_list: true
is_preview: true
---

A .NET Dapr Pluggable Component application can be configured, i.e. for dependency injection, logging, and configuration values, in a manner similar to ASP.NET applications.  The `DaprPluggableComponentsApplcation` exposes a similar set of configuration properties to that exposed by `WebApplicationBuilder`.

## Dependency Injection

Components registered with services can participate in dependency injection; arguments in the components constructor will be injected during creation, assuming those types themselves have been registered with the application, which can be done through the `IServiceCollection` exposed by `DaprPluggableComponentsApplication`.

```csharp
var app = DaprPluggableComponentsApplication.Create();

// Register MyService as the singleton implementation of IService.
app.Services.AddSingleton<IService, MyService>();

app.RegisterService(
    "<service name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MyStateStore>();
    });

app.Run();

interface IService
{
    // ...
}

class MyService : IService
{
    // ...
}

class MyStateStore : IStateStore
{
    // Inject IService on creation of the state store.
    public MyStateStore(IService service)
    {
        // ...
    }

    // ...
}
```

{{% alert title="Warning" color="warning" %}}
Use of `IServiceCollection.AddScoped()` is not recommended as such instances' lifetimes are bound to a single gRPC method call which does not match the lifetime of an individual component instance.
{{% /alert %}}

## Logging

.NET Dapr Pluggable Components can use the [standard .NET logging mechanisms](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging). The `DaprPluggableComponentsApplication` exposes an `ILoggingBuilder` through which it can be configured.

{{% alert title="Note" color="primary" %}}
Like with ASP.NET, logger services (e.g. `ILogger<T>`) are pre-registered.
{{% /alert %}}

```csharp
var app = DaprPluggableComponentsApplication.Create();

// Reset the default loggers and setup new ones.
app.Logging.ClearProviders();
app.Logging.AddConsole();

app.RegisterService(
    "<service name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MyStateStore>();
    });

app.Run();

class MyStateStore : IStateStore
{
    // Inject a logger on creation of the state store.
    public MyStateStore(ILogger<MyStateStore> logger)
    {
        // ...
    }

    // ...
}
```

## Configuration Values

As .NET Dapr Pluggable Components are built on ASP.NET, they can use its [standard configuration mechanisms](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) and default to the same set of [pre-registered providers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#default-application-configuration-sources).  The `DaprPluggableComponentsApplication` exposes an `IConfigurationManager` through which it can be configured.

```csharp
var app = DaprPluggableComponentsApplication.Create();

// Reset the default configuration providers and add new ones.
((IConfigurationBuilder)app.Configuration).Sources.Clear();
app.Configuration.AddEnvironmentVariables();

// Get configuration value on startup.
const value = app.Configuration["<name>"];

app.RegisterService(
    "<service name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MyStateStore>();
    });

app.Run();

class MyStateStore : IStateStore
{
    // Inject the configuration on creation of the state store.
    public MyStateStore(IConfiguration configuration)
    {
        // ...
    }

    // ...
}
```
