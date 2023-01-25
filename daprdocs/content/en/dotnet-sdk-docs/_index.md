---
type: docs
title: "Getting started with the Dapr Pluggable Components .NET SDK"
linkTitle: ".NET"
weight: 1000
description: How to get up and running with the Dapr Pluggable Components .NET SDK
no_list: true
---

Dapr offers NuGet packages to help with the development of .NET Pluggable Components.

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/) or later
- [Dapr 1.9 CLI]({{< ref install-dapr-cli.md >}}) or later
- Initialized [Dapr environment]({{< ref install-dapr-selfhost.md >}})
- Linux, Mac, or Windows (with WSL)

> Development of Dapr Pluggable Components on Windows requires doing so through WSL as some development platforms do not fully support Unix Domain Sockets on "native" Windows.

## Project Creation

Creating a Dapr Pluggable Component starts with an empty ASP.NET project.

```bash
dotnet new web --name <project name>
```

## Add NuGet Packages

Add the Dapr .NET Pluggable Components NuGet package.

```bash
dotnet add package Dapr.PluggableComponents.AspNetCore
```

## Create Application and Service

Creating a Dapr Pluggable Component application is similar to creating an ASP.NET application.  In, `Program.cs`, replace the `WebApplication` related code with the Dapr `DaprPluggableComponentsApplication` equivalent.

```csharp
using Dapr.PluggableComponents;

var app = DaprPluggableComponentsApplication.Create();

app.RegisterService(
    "<socket name>",
    serviceBuilder =>
    {
        // Register one or more components with this service.
    });

app.Run();
```

This creates an application with a single service--each service corresponds to a single Unix Domain Socket, and can host one or more component types.

> Note that only a single component of each type can be registered with an individual service. However, multiple components of the same type can be spread across multiple services.

## Implement and Register Components

 - [Implementing an input/output binding component]({{< ref dotnet-bindings >}})
 - [Implementing a pub-sub component]({{< ref dotnet-pub-sub >}})
 - [Implementing a state store component]({{< ref dotnet-state-store >}})

## Testing Components Locally

Dapr Pluggable Components can be tested by starting the application on the command line and configuring a Dapr sidecar to use it.

To start the component, in the application directory:

```bash
dotnet run
```

To configure Dapr to use the component, in the components path directory:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: <component name>
spec:
  type: state.<socket name>
  version: v1
  metadata:
  - name: key1
    value: value1
  - name: key2
    value: value2
```

To start Dapr (and, optionally, the service making use of the service):

```bash
dapr run --app-id <app id> --components-path <components path> ...
```

At this point, the Dapr sidecar will have started and connected via Unix Domain Socket to the component. You can then interact with the component either through the service using the component (if started) or by using the Dapr HTTP or gRPC API directly.