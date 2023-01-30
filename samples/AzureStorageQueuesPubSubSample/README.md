# Azure Storage Queues Pub-Sub Sample

A Dapr Pluggable Component sample that allows applications to use Azure Storage Queues as a pub-sub component.

## Prerequisites

1. Create an Azure Storage resource for an Azure subscription in the Azure Portal.
1. Create a queue within the storage resource.

## Configuration

An application that binds to the component should have a Pluggable Component configuration file that looks like the following:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: azure-storage-queues
spec:
  type: pubsub.azure-storage-queues
  version: v1
  metadata:
  - name: connectionString
    value: "<connection string>"
  - name: queueName
    value: "<queue name>"
  - name: pollIntervalSeconds
    value: <poll interval in seconds>
  - name: maxMessages
    value: <max messages to receive per poll>
```

The required metadata properties are `<connection string>`, the connection string for the Azure Storage resource, and `<queue name>`, the name of the queue to be polled for messages. The `pollIntervalSeconds` and `maxMessages` properties are optional.

> NOTE: To prevent tokens from being mistakenly committed to a repro, it is best to use Dapr secrets components to store the connection string and then simply reference it from the binding component configuration.

## Application

The application can publish messages to the queue, or subscribe to messages published to the queue, using [HTTP](https://docs.dapr.io/reference/api/pubsub_api/) or the [Dapr Client SDKs](https://docs.dapr.io/developing-applications/sdks/).