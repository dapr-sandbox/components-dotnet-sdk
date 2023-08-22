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

using Dapr.PluggableComponents.Adaptors;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class TransactionalStateStoreAdaptorTests
{
    [Fact]
    public async Task TransactTest()
    {
        var mockStateStore = Substitute.For<ITransactionalStateStore>();

        mockStateStore
            .TransactAsync(Arg.Any<StateStoreTransactRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var logger = Substitute.For<ILogger<TransactionalStateStoreAdaptor>>();

        var mockComponentProvider = Substitute.For<IDaprPluggableComponentProvider<ITransactionalStateStore>>();

        mockComponentProvider
            .GetComponent(Arg.Any<ServerCallContext>())
            .Returns(mockStateStore);

        var adaptor = new TransactionalStateStoreAdaptor(logger, mockComponentProvider);

        using var context = new TestServerCallContext();

        var grpcRequest = new TransactionalStateRequest();

        grpcRequest.Metadata.Add("key", "value");

        await adaptor.Transact(grpcRequest, context);

        await mockStateStore
            .Received(1)
            .TransactAsync(Arg.Is<StateStoreTransactRequest>(request => AssertMetadataEqual(grpcRequest.Metadata, request.Metadata)), Arg.Is<CancellationToken>(cancellationToken => cancellationToken == context.CancellationToken));
    }

    private static bool AssertMetadataEqual(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected, actual);

        return true;
    }
}
