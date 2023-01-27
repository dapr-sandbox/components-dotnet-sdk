---
type: docs
title: "Lifetimes of .NET Dapr Pluggable Components"
linkTitle: "Component Lifetime"
weight: 1000
description: How to control the lifetime of a .NET Pluggable Component
no_list: true
is_preview: true
---

There are two ways to register a components:

 - The component operates as a singleton, with lifetime managed by the SDK
 - A component's lifetime is determined by the Pluggable Component and can be multi-instance or a singleton, as needed

## Singleton Components

Components registered by type are singletons; one instance will serve all configured Dapr components of that type associated with that socket. This approach is best when only a single component of that type exists and shared amongst Dapr applications.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "service-a",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<SingletonStateStore>();
    });

app.Run();

class SingletonStateStore : IStateStore
{
    // ...
}
```

## Multi-instance Components

Components can be registered using by passing a "factory method"; this method will be called for each configured Dapr component of that type associated with that socket. The method returns the instance to associate with that Dapr component (whether shared or not). This approach is best when multiple components of the same type may be configured, for example, with different sets of metadata or when component operations need to be isolated from one another.

The factory method will be passed context, such as the ID of the configured Dapr component, that can be used to differentiate component instances.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "service-a",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore(
            context =>
            {
                return new MultiStateStore(context.InstanceId);
            });
    });

app.Run();

class MultiStateStore : IStateStore
{
    private readonly string instanceId;

    public MultiStateStore(string instanceId)
    {
        this.instanceId = instanceId;
    }

    // ...
}
```

