---
type: docs
title: "Implementing a .NET input/output binding component"
linkTitle: "Bindings"
weight: 1000
description: How to create an input/output binding with the Dapr Pluggable Components .NET SDK
no_list: true
---

## Add Bindings Namespaces

```csharp
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
```

## Implement `IInputBinding`

```csharp
internal sealed class MyBinding : IInputBinding
{
}
```

## Implement `IOutputBinding`

```csharp
internal sealed class MyBinding : IOutputBinding
{
}
```

### Input and Output Bindings Components

A component can be *both* an input *and* output binding, simply by implementing both interfaces.

```csharp
internal sealed class MyBinding : IInputBinding, IOutputBinding
{
}
```

## Register Binding Component

In `Program.cs`, register the binding component in an application service.

```csharp
using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterBinding<MyBinding>();
    });

app.Run();
```
