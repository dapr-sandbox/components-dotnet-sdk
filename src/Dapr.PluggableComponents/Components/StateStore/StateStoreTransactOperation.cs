// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System.Globalization;
using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents an individual transactional operation.
/// </summary>
/// <remarks>
/// To ensure that developers account for all types of operations that might make up a transaction,
/// operation properties are not directly exposed, but instead accessed by one of the Visit() methods.
/// </remarks>
public abstract record StateStoreTransactOperation
{
    /// <summary>
    /// "Visits" the operation, calling the appropriate visitor method based on the operation type.
    /// </summary>
    /// <param name="onDeleteRequest">Called when the operation is a delete request.</param>
    /// <param name="onSetRequest">Called when the operation is a set request.</param>
    public void Visit(Action<StateStoreDeleteRequest> onDeleteRequest, Action<StateStoreSetRequest> onSetRequest)
    {
        this.Visit(
            request =>
            {
                onDeleteRequest(request);

                return true;
            },
            request =>
            {
                onSetRequest(request);

                return true;
            });
    }

    /// <summary>
    /// "Visits" the operation, calling the appropriate visitor method based on the operation type.
    /// </summary>
    /// <typeparam name="TReturn">The type of value being returned from the visit.</typeparam>
    /// <param name="onDeleteRequest">Called when the operation is a delete request.</param>
    /// <param name="onSetRequest">Called when the operation is a set request.</param>
    /// <returns>The value returned from the called visitor method.</returns>
    public abstract TReturn Visit<TReturn>(Func<StateStoreDeleteRequest, TReturn> onDeleteRequest, Func<StateStoreSetRequest, TReturn> onSetRequest);

    internal static StateStoreTransactOperation FromTransactionalStateOperation(TransactionalStateOperation operation)
    {
        return operation.RequestCase switch
        {
            TransactionalStateOperation.RequestOneofCase.Delete => new StateStoreTransactDeleteOperation(StateStoreDeleteRequest.FromDeleteRequest(operation.Delete)),
            TransactionalStateOperation.RequestOneofCase.Set => new StateStoreTransactSetOperation(StateStoreSetRequest.FromSetRequest(operation.Set)),
            _ => throw new ArgumentOutOfRangeException(nameof(operation.RequestCase), String.Format(CultureInfo.CurrentCulture, "The operation type '{0}' is not recognized.", operation.RequestCase))
        };
    }
}

internal sealed record StateStoreTransactDeleteOperation(StateStoreDeleteRequest Request)
    : StateStoreTransactOperation
{
    public override TReturn Visit<TReturn>(Func<StateStoreDeleteRequest, TReturn> onDeleteRequest, Func<StateStoreSetRequest, TReturn> onSetRequest)
    {
        return onDeleteRequest(this.Request);
    }
}

internal sealed record StateStoreTransactSetOperation(StateStoreSetRequest Request)
    : StateStoreTransactOperation
{
    public override TReturn Visit<TReturn>(Func<StateStoreDeleteRequest, TReturn> onDeleteRequest, Func<StateStoreSetRequest, TReturn> onSetRequest)
    {
        return onSetRequest(this.Request);
    }
}
