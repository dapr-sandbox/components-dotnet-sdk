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
using Dapr.PluggableComponents.Components.StateStore;

namespace Dapr.PluggableComponents;

public sealed record ComponentProviderContext(string? InstanceId, IServiceProvider ServiceProvider, string SocketPath);

public delegate T ComponentProviderDelegate<T>(ComponentProviderContext context);

public sealed class DaprPluggableComponentsServiceBuilder
{
    private readonly string socketPath;
    private readonly IDaprPluggableComponentsRegistrar registrar;

    internal DaprPluggableComponentsServiceBuilder(
        string socketPath,
        IDaprPluggableComponentsRegistrar registrar)
    {
        this.socketPath = socketPath;
        this.registrar = registrar;
    }

    public DaprPluggableComponentsServiceBuilder RegisterStateStore<TStateStore>() where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>();

        return this;
    }

    public DaprPluggableComponentsServiceBuilder RegisterStateStore<TStateStore>(ComponentProviderDelegate<TStateStore> stateStoreFactory)
        where TStateStore : class, IStateStore
    {
        this.AddComponent<IStateStore, TStateStore, StateStoreAdaptor>(stateStoreFactory);

        return this;
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>()
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.registrar.RegisterComponent<TComponentImpl>(this.socketPath);

        this.AddRelatedService<TComponentType, TComponentImpl, TAdaptor>();
    }

    private void AddComponent<TComponentType, TComponentImpl, TAdaptor>(ComponentProviderDelegate<TComponentImpl> pubSubFactory)
        where TComponentType : class
        where TComponentImpl : class, TComponentType
        where TAdaptor : class
    {
        this.registrar.RegisterComponent<TComponentImpl>(socketPath, pubSubFactory);

        this.AddRelatedService<TComponentType, TComponentImpl, TAdaptor>();
    }

    private void AddRelatedService<TComponent, TComponentImpl, TAdaptor>()
        where TComponent : class
        where TComponentImpl : class
        where TAdaptor : class
    {
        this.registrar.RegisterProvider<TComponent, TComponentImpl>(this.socketPath);

        this.registrar.RegisterAdaptor<TAdaptor>();
    }
}
