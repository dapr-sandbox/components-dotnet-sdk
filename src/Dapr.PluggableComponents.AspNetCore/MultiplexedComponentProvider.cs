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

using System.Collections.Concurrent;
using Dapr.PluggableComponents.Adaptors;
using Grpc.Core;

namespace Dapr.PluggableComponents;

internal sealed class MultiplexedComponentProvider<T> : IDaprPluggableComponentProvider<T>
{
    private const string MetadataInstanceId = "x-component-instance";

    private readonly ComponentProviderDelegate<T> componentProvider;
    private readonly ConcurrentDictionary<string, Lazy<T>> components = new ConcurrentDictionary<string, Lazy<T>>();
    private readonly Lazy<T> defaultComponent;
    private readonly ComponentProviderContext defaultComponentProviderContext;

    public MultiplexedComponentProvider(ComponentProviderDelegate<T> componentProvider, IServiceProvider serviceProvider, string socketPath)
    {
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));

        this.defaultComponentProviderContext = new ComponentProviderContext(
            null,
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
            socketPath ?? throw new ArgumentNullException(nameof(socketPath)));

        this.defaultComponent = new Lazy<T>(() => componentProvider(this.defaultComponentProviderContext));
    }

    #region IDaprPluggableComponentProvider<T> Members

    public T GetComponent(ServerCallContext context)
    {
        var entry = context.RequestHeaders.Get(MetadataInstanceId);

        var component =
            entry != null
                ? this.components.GetOrAdd(entry.Value, instanceId => new Lazy<T>(() => this.componentProvider(this.defaultComponentProviderContext with { InstanceId = instanceId })))
                : this.defaultComponent;

        return component.Value;
    }

    #endregion
}
