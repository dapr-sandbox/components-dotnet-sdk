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

using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreTransactRequestTests
{
    [Fact]
    public void FromTransactionalStateRequestConversionTests()
    {
        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var request = new TransactionalStateRequest();

                request.Metadata.Add(metadata);

                return request;
            },
            StateStoreTransactRequest.FromTransactionalStateRequest,
            request => request.Metadata);
    }

    [Fact]
    public void FromTransactionalStateRequestOperationsTests()
    {
        var grpcRequest = new TransactionalStateRequest();

        string key1 = "key1";
        string key2 = "key2";

        grpcRequest.Operations.Add(new TransactionalStateOperation { Delete = new DeleteRequest { Key = key1 } });
        grpcRequest.Operations.Add(new TransactionalStateOperation { Set = new SetRequest { Key = key2 } });

        var request = StateStoreTransactRequest.FromTransactionalStateRequest(grpcRequest);

        Assert.NotNull(request);
        Assert.Equal(2, request.Operations.Length);

        var operation1 = request.Operations[0];
        var operation2 = request.Operations[1];

        Assert.Equal(key1, operation1.Visit<StateStoreDeleteRequest?>(deleteRequest => deleteRequest, setRequest => null)?.Key);
        Assert.Equal(key2, operation2.Visit<StateStoreSetRequest?>(deleteRequest => null, setRequest => setRequest)?.Key);
    }
}
