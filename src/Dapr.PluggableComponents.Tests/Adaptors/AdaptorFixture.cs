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
using Dapr.PluggableComponents.Components.SecretStore;
using Dapr.PluggableComponents.Components.StateStore;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dapr.PluggableComponents.Adaptors;

internal sealed class AdaptorFixture<TAdaptor, TInterface> : AdaptorFixture, IDisposable
    where TInterface : class, IPluggableComponent
{
    private readonly Lazy<TAdaptor> adaptor;

    public AdaptorFixture(Func<ILogger<TAdaptor>, IDaprPluggableComponentProvider<TInterface>, TAdaptor> adaptorFactory, TInterface? mockComponent = null)
    {
        this.MockComponent = mockComponent ?? Substitute.For<TInterface>();

        this.adaptor = new Lazy<TAdaptor>(() => Create<TAdaptor, TInterface>(this.MockComponent, adaptorFactory));
    }

    /// <remarks>
    /// Loads lazily to ensure that clients have the ability to add interfaces to the mock component.
    /// (Interfaces cannot be added after first use of Mock<T>.Object.)
    /// </remarks>
    public TAdaptor Adaptor => this.adaptor.Value;

    public TestServerCallContext Context { get; } = new TestServerCallContext();

    public TInterface MockComponent { get; }

    #region IDisposable Members

    public void Dispose()
    {
        this.Context.Dispose();
    }

    #endregion
}

internal abstract class AdaptorFixture
{
    public static AdaptorFixture<InputBindingAdaptor, IInputBinding> CreateInputBinding(IInputBinding? mockComponent = null)
    {
        return new AdaptorFixture<InputBindingAdaptor, IInputBinding>((logger, componentProvider) => new InputBindingAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<OutputBindingAdaptor, IOutputBinding> CreateOutputBinding(IOutputBinding? mockComponent = null)
    {
        return new AdaptorFixture<OutputBindingAdaptor, IOutputBinding>((logger, componentProvider) => new OutputBindingAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<PubSubAdaptor, IPubSub> CreatePubSub(IPubSub? mockComponent = null)
    {
        return new AdaptorFixture<PubSubAdaptor, IPubSub>((logger, componentProvider) => new PubSubAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<StateStoreAdaptor, IStateStore> CreateStateStore(IStateStore? mockComponent = null)
    {
        return new AdaptorFixture<StateStoreAdaptor, IStateStore>((logger, componentProvider) => new StateStoreAdaptor(logger, componentProvider), mockComponent);
    }

    public static AdaptorFixture<SecretStoreAdaptor, ISecretStore> CreateSecretStore(ISecretStore? mockComponent = null)
    {
        return new AdaptorFixture<SecretStoreAdaptor, ISecretStore>((logger, componentProvider) => new SecretStoreAdaptor(logger, componentProvider), mockComponent);
    }

    public static async Task TestInitAsync<TAdaptor, TInterface>(
        Func<AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Client.Autogen.Grpc.v1.MetadataRequest, Task> initCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory();

        fixture.MockComponent
            .InitAsync(Arg.Any<Components.MetadataRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var properties = new Dictionary<string, string>()
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var metadataRequest = new Client.Autogen.Grpc.v1.MetadataRequest();

        metadataRequest.Properties.Add(properties);

        await initCall(fixture, metadataRequest);

        await fixture.MockComponent
            .Received(1)
            .InitAsync(
                    Arg.Is<Components.MetadataRequest>(request => ConversionAssert.MetadataEqual(properties, request.Properties)),
                    Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }

    public static async Task TestPingAsync<TAdaptor, TInterface>(
        Func<TInterface?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        await PingWithNoLiveness<TAdaptor, TInterface>(adaptorFactory, pingCall);
        await PingWithLiveness<TAdaptor, TInterface>(adaptorFactory, pingCall);
    }

    private static async Task PingWithNoLiveness<TAdaptor, TInterface>(
        Func<TInterface?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory(Substitute.For<TInterface>());

        await pingCall(fixture);
    }

    private static async Task PingWithLiveness<TAdaptor, TInterface>(
        Func<TInterface?, AdaptorFixture<TAdaptor, TInterface>> adaptorFactory,
        Func<AdaptorFixture<TAdaptor, TInterface>, Task> pingCall)
        where TInterface : class, IPluggableComponent
    {
        using var fixture = adaptorFactory(Substitute.For<TInterface, IPluggableComponentLiveness>());

        var mockLiveness = (IPluggableComponentLiveness)fixture.MockComponent;

        mockLiveness
            .PingAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await pingCall(fixture);

        await mockLiveness
            .Received(1)
            .PingAsync(Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }

    protected static TAdaptor Create<TAdaptor, TInterface>(TInterface component, Func<ILogger<TAdaptor>, IDaprPluggableComponentProvider<TInterface>, TAdaptor> adaptorFactory)
    {
        var logger = Substitute.For<ILogger<TAdaptor>>();

        var mockComponentProvider = Substitute.For<IDaprPluggableComponentProvider<TInterface>>();

        mockComponentProvider
            .GetComponent(Arg.Any<ServerCallContext>())
            .Returns(component);

        return adaptorFactory(logger, mockComponentProvider);
    }
}
