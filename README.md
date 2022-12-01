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
var componentName = "<component name>";

var options = new DaprPluggableComponentsOptions
    {
        Args = args,
        SocketName = componentName
    };

var app = DaprPluggableComponentsApplication.Create(options);

app.Run();
```

Implement one or more component interfaces.

- `Dapr.PluggableComponents.Bindings.IInputBinding`: The interface for input binding components.
- `Dapr.PluggableComponents.Bindings.IOuputBinding`: The interface for output binding components.
- `Dapr.PluggableComponents.PubSub.IPubSub`: The interface for pub-sub components.
- `Dapr.PluggableComponents.StateStore.IStateStore`: The interface for state store components.

Register one or more component types.

> NOTE: Only a single component of any given type can be registered.

```csharp
var componentName = "<component name>";

var options = new DaprPluggableComponentsOptions
    {
        Args = args,
        SocketName = componentName
    };

var app = DaprPluggableComponentsApplication.Create(options);

app.AddInputBinding<MyInputBinding>     // Add an input binding.
app.AddOutputBinding<MyOutputBinding>   // Add an output binding.
app.AddPubSub<MyPubSub>                 // Add a pub-sub component.
app.AddStateStore<MyStateStore>         // Add a state store.

app.Run();

class MyStateStore : IStateStore
{
    // ...
}
```

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