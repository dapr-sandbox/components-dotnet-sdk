---
type: docs
title: "Multiple services in a .NET Dapr Pluggable Component"
linkTitle: "Multiple services"
weight: 1000
description: How to expose multiple services from a .NET Pluggable Component
no_list: true
is_preview: true
---

A Dapr Pluggable Component can host multiple components of varying types. This might be done for efficiency reasons, to minimize the number of sidecars running in a cluster. It might also be done for implementation reasons, to group related components such as a database exposed both as a general state store as well as output bindings that allow more specific operations, where components are likely to share libraries and implementation.

Each Unix Domain Socket can manage calls to, at most, one component of each type. To host multiple components of the *same* type, those types can be spread across multiple sockets. The SDK binds each socket to a "service", with each service composed of one or more component types.

## Registering Multiple Services

Each call to `RegisterService()` binds a socket to a set of registered components, where, at most, one of each type of component can be registered per service.

```csharp
var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "service-a",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<MyDatabaseStateStore>();
        serviceBuilder.RegisterBinding<MyDatabaseOutputBinding>();
    });

app.RegisterService(
    "service-b",
    serviceBuilder =>
    {
        serviceBuilder.RegisterStateStore<AnotherStateStore>();
    });

app.Run();

class MyDatabaseStateStore : IStateStore
{
    // ...
}

class MyDatabaseOutputBinding : IOutputBinding
{
    // ...
}

class AnotherStateStore : IStateStore
{
    // ...
}
```

## Configuring Multiple Components

Configuring Dapr to use the hosted components is the same as for any single component; the component YAML just refers to the associated socket.

```yaml
#
# This component uses the state store associated with socket `state-store-a`
#
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: state-store-a
spec:
  type: state.service-a
  version: v1
  metadata: []
```

```yaml
#
# This component uses the state store associated with socket `state-store-b`
#
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: state-store-b
spec:
  type: state.service-b
  version: v1
  metadata: []
```
