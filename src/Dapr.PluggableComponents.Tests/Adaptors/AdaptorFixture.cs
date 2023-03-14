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

using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.PluggableComponents.Components.PubSub;
using Dapr.PluggableComponents.Components.StateStore;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dapr.PluggableComponents.Adaptors;

internal sealed class AdaptorFixture<TAdaptor, TInterface> : AdaptorFixture, IDisposable
    where TInterface : class, IPluggableComponent
{
    private readonly TestServerCallContext context = new TestServerCallContext();
    private readonly Lazy<TAdaptor> adaptor;

    public AdaptorFixture(Func<ILogger<TAdaptor>, IDaprPluggableComponentProvider<TInterface>, TAdaptor> adaptorFactory, Mock<TInterface>? mockComponent = null)
    {
        this.MockComponent = mockComponent ?? new Mock<TInterface>();

        this.adaptor = new Lazy<TAdaptor>(() => Create<TAdaptor, TInterface>(this.MockComponent.Object, adaptorFactory));
    }

    /// <remarks>
    /// Loads lazily to ensure that clients have the ability to add interfaces to the mock component.
    /// (Interfaces cannot be added after first use of Mock<T>.Object.)
    /// </remarks>
    public TAdaptor Adaptor => this.adaptor.Value;

    public ServerCallContext Context => this.context;

    public Mock<TInterface> MockComponent { get; }

    #region IDisposable Members

    public void Dispose()
    {
        this.context.Dispose();
    }

    #endregion
}

internal abstract class AdaptorFixture
{
    public static AdaptorFixture<OutputBindingAdaptor, IOutputBinding> CreateOutputBinding(Mock<IOutputBinding>? mockComponent = null)
    {
        return new AdaptorFixture<OutputBindingAdaptor, IOutputBinding>((logger, componentProvider) => new OutputBindingAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<PubSubAdaptor, IPubSub> CreatePubSub(Mock<IPubSub>? mockComponent = null)
    {
        return new AdaptorFixture<PubSubAdaptor, IPubSub>((logger, componentProvider) => new PubSubAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<StateStoreAdaptor, IStateStore> CreateStateStore(Mock<IStateStore>? mockComponent = null)
    {
        return new AdaptorFixture<StateStoreAdaptor, IStateStore>((logger, componentProvider) => new StateStoreAdaptor(logger, componentProvider), mockComponent);
    }

    public static async Task TestInitAsync<TAdaptor, TInterface>(
        Func<AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Client.Autogen.Grpc.v1.MetadataRequest, Task> initCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory();

        fixture.MockComponent
            .Setup(component => component.InitAsync(It.IsAny<Components.MetadataRequest>(), It.IsAny<CancellationToken>()));

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new Client.Autogen.Grpc.v1.MetadataRequest();

        metadataRequest.Properties.Add(properties);

        await initCall(fixture, metadataRequest);

        fixture.MockComponent
            .Verify(
                component => component.InitAsync(
                    It.Is<Components.MetadataRequest>(request => ConversionAssert.MetadataEqual(properties, request.Properties)),
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    public static async Task TestPingAsync<TAdaptor, TInterface>(
        Func<Mock<TInterface>?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        await PingWithNoLiveness<TAdaptor, TInterface>(adaptorFactory, pingCall);
        await PingWithLiveness<TAdaptor, TInterface>(adaptorFactory, pingCall);
    }

    private static async Task PingWithNoLiveness<TAdaptor, TInterface>(
        Func<Mock<TInterface>?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory(new Mock<TInterface>(MockBehavior.Strict));

        await pingCall(fixture);
    }

    private static async Task PingWithLiveness<TAdaptor, TInterface>(
        Func<Mock<TInterface>?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory(null);

        var mockLiveness = fixture.MockComponent.As<IPluggableComponentLiveness>();

        mockLiveness
            .Setup(component => component.PingAsync(It.IsAny<CancellationToken>()));

        await pingCall(fixture);

        mockLiveness
            .Verify(
                component => component.PingAsync(
                    It.Is<CancellationToken>(token => token == fixture.Context.CancellationToken)),
                Times.Once());
    }

    protected static TAdaptor Create<TAdaptor, TInterface>(TInterface component, Func<ILogger<TAdaptor>, IDaprPluggableComponentProvider<TInterface>, TAdaptor> adaptorFactory)
    {
        var logger = new Mock<ILogger<TAdaptor>>();

        var mockComponentProvider = new Mock<IDaprPluggableComponentProvider<TInterface>>();

        mockComponentProvider
            .Setup(componentProvider => componentProvider.GetComponent(It.IsAny<ServerCallContext>()))
            .Returns(component);

        return adaptorFactory(logger.Object, mockComponentProvider.Object);
    }
}
