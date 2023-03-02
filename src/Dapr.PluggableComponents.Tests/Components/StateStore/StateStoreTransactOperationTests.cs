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

using Dapr.Proto.Components.V1;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreTransactOperationTests
{
    [Fact]
    public void FromTransactionalStateOperationDelete()
    {
        string key = "key";

        var grpcOperation = new TransactionalStateOperation
        {
            Delete = new DeleteRequest { Key = key }
        };

        var operation = StateStoreTransactOperation.FromTransactionalStateOperation(grpcOperation);

        var request = operation.Visit<StateStoreDeleteRequest?>(
            deleteRequest => deleteRequest,
            setRequest => null);

        Assert.NotNull(request);
        Assert.Equal(key, request.Key);
    }

    [Fact]
    public void FromTransactionalStateOperationSet()
    {
        string key = "key";

        var grpcOperation = new TransactionalStateOperation
        {
            Set = new SetRequest { Key = key }
        };

        var operation = StateStoreTransactOperation.FromTransactionalStateOperation(grpcOperation);

        var request = operation.Visit<StateStoreSetRequest?>(
            deleteRequest => null,
            setRequest => setRequest);

        Assert.NotNull(request);
        Assert.Equal(key, request.Key);
    }

    [Fact]
    public void FromTransactionalStateOperationUnknown()
    {
        var grpcOperation = new TransactionalStateOperation();

        Assert.Throws<ArgumentOutOfRangeException>(() => StateStoreTransactOperation.FromTransactionalStateOperation(grpcOperation));
    }
}
