---
type: docs
title: "Implementing a .NET pub-sub component"
linkTitle: "Pub-Sub"
weight: 1000
description: How to create a pub-sub with the Dapr Pluggable Components .NET SDK
no_list: true
is_preview: true
---

## Add Pub-Sub Namespaces

```csharp
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;
```

## Implement `IPubSub`

```csharp
internal sealed class MyPubSub : IPubSub
{
    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
    }

    public async Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = default)
    {
    }

    public async Task PullMessagesAsync(PubSubPullMessagesTopic topic, MessageDeliveryHandler handler, CancellationToken cancellationToken = default)
    {
    }
}
```

## Register Pub-Sub Component

In `Program.cs`, register the pub-sub component in an application service.

```csharp
using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        serviceBuilder.RegisterPubSub<MyPubSub>();
    });

app.Run();
```
