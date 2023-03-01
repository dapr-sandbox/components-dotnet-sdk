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
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class TransactionalStateStoreAdaptorTests
{
    [Fact]
    public async Task TransactTest()
    {
        var mockStateStore = new Mock<ITransactionalStateStore>();

        mockStateStore
            .Setup(stateStore => stateStore.TransactAsync(It.IsAny<StateStoreTransactRequest>(), It.IsAny<CancellationToken>()));

        var logger = new Mock<ILogger<TransactionalStateStoreAdaptor>>();

        var mockComponentProvider = new Mock<IDaprPluggableComponentProvider<ITransactionalStateStore>>();

        mockComponentProvider
            .Setup(componentProvider => componentProvider.GetComponent(It.IsAny<ServerCallContext>()))
            .Returns(mockStateStore.Object);

        var adaptor = new TransactionalStateStoreAdaptor(logger.Object, mockComponentProvider.Object);

        using var context = new TestServerCallContext();

        var grpcRequest = new TransactionalStateRequest();

        grpcRequest.Metadata.Add("key", "value");

        await adaptor.Transact(grpcRequest, context);

        mockStateStore
            .Verify(stateStore => stateStore.TransactAsync(It.Is<StateStoreTransactRequest>(request => AssertMetadataEqual(grpcRequest.Metadata, request.Metadata)), It.Is<CancellationToken>(cancellationToken => cancellationToken == context.CancellationToken)), Times.Once());
    }

    private static bool AssertMetadataEqual(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected, actual);

        return true;
    }
}