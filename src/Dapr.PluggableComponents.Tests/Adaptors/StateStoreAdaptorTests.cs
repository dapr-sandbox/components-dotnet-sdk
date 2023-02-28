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

using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components.StateStore;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class StateStoreAdaptorTests
{
    [Fact]
    public async Task InitTests()
    {
        var logger = new Mock<ILogger<StateStoreAdaptor>>();

        var mockComponentProvider = new Mock<IDaprPluggableComponentProvider<IStateStore>>();
        var mockComponent = new Mock<IStateStore>();

        mockComponentProvider
            .Setup(componentProvider => componentProvider.GetComponent(It.IsAny<ServerCallContext>()))
            .Returns(mockComponent.Object);

        mockComponent
            .Setup(component => component.InitAsync(It.IsAny<Components.MetadataRequest>(), It.IsAny<CancellationToken>()));

        var adaptor = new StateStoreAdaptor(logger.Object, mockComponentProvider.Object);

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new MetadataRequest();

        metadataRequest.Properties.Add(properties);

        using var context = new TestServerCallContext();

        await adaptor.Init(
            new Proto.Components.V1.InitRequest
            {
                Metadata = metadataRequest
            },
            context);

        mockComponentProvider
            .Verify(componentProvider => componentProvider.GetComponent(It.IsAny<ServerCallContext>()), Times.Once());

        mockComponent
            .Verify(
                component => component.InitAsync(
                    It.Is<Components.MetadataRequest>(request => AssertMetadataEqual(properties, request.Properties)),
                    It.Is<CancellationToken>(token => token == context.CancellationToken)),
                Times.Once());
    }

    private static bool AssertMetadataEqual(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected, actual);

        return true;
    }

    private sealed class TestServerCallContext : ServerCallContext, IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        #region ServerCallContext Overrides

        protected override string MethodCore => throw new NotImplementedException();

        protected override string HostCore => throw new NotImplementedException();

        protected override string PeerCore => throw new NotImplementedException();

        protected override DateTime DeadlineCore => throw new NotImplementedException();

        protected override Metadata RequestHeadersCore => throw new NotImplementedException();

        protected override CancellationToken CancellationTokenCore => this.cts.Token;

        protected override Metadata ResponseTrailersCore => throw new NotImplementedException();

        protected override Status StatusCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override AuthContext AuthContextCore => throw new NotImplementedException();

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.cts.Dispose();
        }

        #endregion
    }
}
