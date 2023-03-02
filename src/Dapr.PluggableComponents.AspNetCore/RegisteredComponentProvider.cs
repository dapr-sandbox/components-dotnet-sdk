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

using System.Globalization;
using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.AspNetCore;
using Grpc.Core;
using Microsoft.AspNetCore.Connections.Features;

namespace Dapr.PluggableComponents;

internal sealed class RegisteredComponentProvider<TComponent> : IDaprPluggableComponentProvider<TComponent>
{
    private readonly DaprPluggableComponentsRegistry registry;
    private readonly IServiceProvider serviceProvider;

    public RegisteredComponentProvider(DaprPluggableComponentsRegistry registry, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.registry = registry;
        this.serviceProvider = serviceProvider;
    }

    public TComponent GetComponent(ServerCallContext context)
    {
        string socketPath = GetSocketPath(context);

        var componentProvider = this.registry.GetComponentProvider<TComponent>(this.serviceProvider, socketPath)
            ?? throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.RegisteredComponentProviderNoProviderMessage, typeof(TComponent)));

        return componentProvider.GetComponent(context);
    }

    private static string GetSocketPath(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var socketFeature = httpContext.Features.Get<IConnectionSocketFeature>();
        var socketPath = socketFeature?.Socket.LocalEndPoint?.ToString() ?? throw new InvalidOperationException(Resources.RegisteredComponentProviderUnknownSocketMessage);

        return socketPath;
    }
}
