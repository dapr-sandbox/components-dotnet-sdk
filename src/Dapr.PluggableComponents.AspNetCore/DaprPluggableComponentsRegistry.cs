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
using System.Globalization;
using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.AspNetCore;

namespace Dapr.PluggableComponents;

internal sealed class DaprPluggableComponentsRegistry
{
    private readonly ConcurrentDictionary<string, SocketRegistry> registries = new();

    public void RegisterComponentProvider<TComponent>(string socketPath, Func<IServiceProvider, IDaprPluggableComponentProvider<TComponent>> providerFactory)
        where TComponent : class
    {
        var registry = this.registries.GetOrAdd(socketPath, _ => new SocketRegistry());

        if (!registry.RegisteredTypes.TryAdd(typeof(TComponent), new CachedComponentProvider(providerFactory)))
        {
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DaprPluggableComponentsRegistryDuplicateTypeMessage, typeof(TComponent), socketPath), nameof(providerFactory));
        }
    }

    public void RegisterComponentProvider<TComponent, TComponentImpl>(string socketPath)
        where TComponentImpl : class
    {
        var registry = this.registries.GetOrAdd(socketPath, _ => new SocketRegistry());

        if (!registry.RegisteredTypes.TryAdd(typeof(TComponent),
            new CachedComponentProvider(
            serviceProvider =>
            {
                var componentProvider = this.GetComponentProvider<TComponentImpl>(serviceProvider, socketPath);

                return componentProvider;
            })))
        {
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DaprPluggableComponentsRegistryDuplicateTypeMessage, typeof(TComponent), socketPath), nameof(socketPath));
        }
    }

    public IDaprPluggableComponentProvider<T> GetComponentProvider<T>(IServiceProvider serviceProvider, string socketPath)
    {
        if (this.registries.TryGetValue(socketPath, out var registry)
            && registry.RegisteredTypes.TryGetValue(typeof(T), out var providerFactory)
            && providerFactory.GetComponentProvider(serviceProvider) is IDaprPluggableComponentProvider<T> componentProvider)
        {
            return componentProvider;
        }

        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.DaprPluggableComponetnsRegistryProviderNotFoundMessage, typeof(T)));
    }

    private sealed class SocketRegistry
    {
        public ConcurrentDictionary<Type, CachedComponentProvider> RegisteredTypes { get; } = new ConcurrentDictionary<Type, CachedComponentProvider>();
    }

    private sealed class CachedComponentProvider
    {
        private IDaprPluggableComponentProvider<object>? componentProvider;
        private readonly Func<IServiceProvider, IDaprPluggableComponentProvider<object>> componentProviderFactory;
        private readonly object componentProviderLock = new();

        public CachedComponentProvider(Func<IServiceProvider, IDaprPluggableComponentProvider<object>> componentProviderFactory)
        {
            this.componentProviderFactory = componentProviderFactory ?? throw new ArgumentNullException(nameof(componentProviderFactory));
        }

        public IDaprPluggableComponentProvider<object> GetComponentProvider(IServiceProvider serviceProvider)
        {
            if (this.componentProvider == null)
            {
                lock (this.componentProviderLock)
                {
                    this.componentProvider ??= this.componentProviderFactory(serviceProvider);
                }
            }

            return this.componentProvider;
        }
    }
}
