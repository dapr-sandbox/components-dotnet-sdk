---
type: docs
title: "Implementing a .NET state store component"
linkTitle: "State Store"
weight: 1000
description: How to create a state store with the Dapr Pluggable Components .NET SDK
no_list: true
is_preview: true
---

Creating a state store component requires just a few basic steps.

## Add State Store Namespaces

Add `using` statements for the state store related namespaces.

```csharp
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.StateStore;
```

## Implement `IStateStore`

Create a class that implements the `IStateStore` interface.

```csharp
internal sealed class MyStateStore : IStateStore
{
    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        // Delete the requested key from the state store...
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        // Get the requested key value from from the state store, else return null...
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        // Called to initialize the component with its configured metadata...
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        // Set the requested key to the specified value in the state store...
    }
}
```

## Register State Store Component

In the main program file (e.g. `Program.cs`), register the state store with an application service.

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

## Bulk State Stores

State stores that intend to support bulk operations should implement the optional `IBulkStateStore` interface. Its methods mirror those of the base `IStateStore` interface, but include multiple requested values.

{{% alert title="Note" color="primary" %}}
The Dapr runtime will emulate bulk state store operations for state stores that do *not* implement `IBulkStateStore` by calling its operations individually.
{{% /alert %}}

```csharp
internal sealed class MyStateStore : IStateStore, IBulkStateStore
{
    // ...

    public Task BulkDeleteAsync(StateStoreDeleteRequest[] requests, CancellationToken cancellationToken = default)
    {
        // Delete all of the requested values from the state store...
    }

    public Task<StateStoreBulkStateItem[]> BulkGetAsync(StateStoreGetRequest[] requests, CancellationToken cancellationToken = default)
    {
        // Return the values of all of the requested values from the state store...
    }

    public Task BulkSetAsync(StateStoreSetRequest[] requests, CancellationToken cancellationToken = default)
    {
        // Set all of the values of the requested keys in the state store...
    }
}
```

## Transactional State Stores

State stores that intend to support transactions should implement the optional `ITransactionalStateStore` interface. Its `TransactAsync()` method is passed a request with a sequence of delete and/or set operations to be performed within a transaction. The state store should iterate over the sequence and call each operation's `Visit()` method, passing callbacks that represent the action to take for each type of operation.

```csharp
internal sealed class MyStateStore : IStateStore, ITransactionalStateStore
{
    // ...

    public async Task TransactAsync(StateStoreTransactRequest request, CancellationToken cancellationToken = default)
    {
        // Start transaction...

        try
        {
            foreach (var operation in request.Operations)
            {
                await operation.Visit(
                    async deleteRequest =>
                    {
                        // Process delete request...

                    },
                    async setRequest =>
                    {
                        // Process set request...
                    });
            }
        }
        catch
        {
            // Rollback transaction...

            throw;
        }

        // Commit transaction...
    }
}
```

## Queryable State Stores

State stores that intend to support queries should implement the optional `IQueryableStateStore` interface. Its `QueryAsync()` method is passed details about the query, such as the filter(s), result limits and pagination, and sort order(s) of the results. The state store should use those details to generate a set of values to return as part of its response.

```csharp
internal sealed class MyStateStore : IStateStore, IQueryableStateStore
{
    // ...

    public Task<StateStoreQueryResponse> QueryAsync(StateStoreQueryRequest request, CancellationToken cancellationToken = default)
    {
        // Generate and return results...
    }
}
```

## ETag and Other Semantic Error Handling

The Dapr runtime has additional handling of certain error conditions resulting from some state store operations. State stores can indicate such conditions by throwing specific exceptions from its operation logic:

| Exception | Applicable Operations | Description
|---|---|---|
| `ETagInvalidException` | Delete, Set, Bulk Delete, Bulk Set | When an ETag is invalid |
| `ETagMismatchException`| Delete, Set, Bulk Delete, Bulk Set | When an ETag does not match an expected value |
| `BulkDeleteRowMismatchException` | Bulk Delete | When the number of affected rows does not match the expected rows |
