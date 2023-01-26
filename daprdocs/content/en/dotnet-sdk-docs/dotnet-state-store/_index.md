---
type: docs
title: "Implementing a .NET state store component"
linkTitle: "State Store"
weight: 1000
description: How to create a state store with the Dapr Pluggable Components .NET SDK
no_list: true
is_preview: true
---

## Add State Store Namespaces

```csharp
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
```

## Implement `IStateStore`

```csharp
internal sealed class MyStateStore : IStateStore
{
    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
    }
}
```

## Register State Store Component

In `Program.cs`, register the state store in an application service.

```csharp
using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MyStateStore>();
    });

app.Run();
```
