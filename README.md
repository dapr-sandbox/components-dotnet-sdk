# Dapr Pluggable Components SDK for .NET

[Dapr Pluggable Components](https://docs.dapr.io/concepts/components-concept/#built-in-and-pluggable-components) are Dapr components that reside outside the Dapr runtime but are dynamically registered with the runtime.

This SDK provides a better interface to create Pluggable Components without worrying about the underlying communication protocols and connection resiliency.

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/) or later
- [Dapr 1.9](https://dapr.io/) or later

## Implementing a Pluggable Component

Create an empty ASP.NET project.

```bash
> dotnet new web --name <project name>
```

Add the Dapr Pluggable Components SDK NuGet package.

```bash
> dotnet add package Dapr.PluggableComponents.AspNetCore
```

In `Program.cs`, Replace the default application building logic with Dapr equivalents.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        // TODO: Register Dapr Pluggable Components.
    });

app.Run();
```

Implement one or more component interfaces.

- `Dapr.PluggableComponents.StateStore.IStateStore`: The interface for state store components.

Register one or more component types with the service.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        // Register a state store with the service.
        serviceBuilder.RegisterStateStore<MyStateStore>();
    });

app.Run();

class MyStateStore : IStateStore
{
    // ...
}
```

> NOTE: Only a single component of any given type can be registered with a given service.  However, you can register components of the same type on separate services, and you can have as many services as you need.

## Registering a Pluggable Component

Once implemented, a Pluggable Component must be [registered as a component](https://docs.dapr.io/operations/components/pluggable-components/pluggable-components-registration/) of a Dapr application.

In standalone mode, this might look like:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: <component name>
spec:
  type: <component type>.<socket name>
  version: v1
  metadata:
```

In this example:

- `<component name>` represents the name of the Dapr component.
- `<component type>` represents the type of the Dapr component.
- `<socket name>` represents the name of the socket associated with the Pluggable Component application hosting the Dapr component.

## Pluggable Component Customization

### Per-Component Instances

By default, pluggable component implementations are singletons, so every invocation of a Dapr component will invoke the same Pluggable Component instance.  In some cases it might be preferable to have multiple instances serve each component.  This can be achieved by registering the component using a provider delegate.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        // Register a state store with the service.
        serviceBuilder.RegisterStateStore<MyStateStore>(
            context =>
            {
                // Return a new state store instance for each individual Dapr component.
                return new MyStateStore();
            });
    });

app.Run();

class MyStateStore : IStateStore
{
    // ...
}
```

The provider delegate is passed a context argument that allows delegate logic to determine how to create/return the component instance, such as the Dapr component instance ID and the socket path of the service.

### Multiple Services

Pluggable Components of the same type can be exposed from the same application at the same time by using separate services, each of which are hosted from a separate socket.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket A>",
    serviceBuilder =>
    {
        // Register a state store with the service.
        serviceBuilder.RegisterStateStore<StateStoreA>();
    });

app.RegisterService(
    "<socket B>",
    serviceBuilder =>
    {
        // Register a state store with the service.
        serviceBuilder.RegisterStateStore<StateStoreB>();
    });

app.Run();

class StateStoreA : IStateStore
{
    // ...
}

class StateStoreB : IStateStore
{
    // ...
}
```

### Socket Path Customization

Registering a service requires only the name of the socket to be created in Dapr's default location. In cases where you need to change path of the socket, you can pass an options argument when registering the service.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    new DaprPluggableComponentsServiceOptions("<socket name>")
    {
        SocketExtension = ".mysock",
        SocketFolder = "/tmp/my-components"
    },
    serviceBuilder =>
        // ...
    });

app.Run();
```

### Dependency Injection, Logging, and Configuration

A Dapr Pluggable Component application can make use of dependency injection, logging, and configuration, similar to ASP.NET applications.  These are exposed via properties on `DaprPluggableComponentsApplication`.

```csharp
var app = DaprPluggableComponentsApplication.Create();

// Add configuration providers...
app.Configuration.AddJsonFile("config.json");

// Add logging sinks...
app.Logging.AddConsole();

// Register services...
app.Services.AddSingleton<MyService>();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        // ...
    });

app.Run();

class MyService
{
    // ...
}
```
