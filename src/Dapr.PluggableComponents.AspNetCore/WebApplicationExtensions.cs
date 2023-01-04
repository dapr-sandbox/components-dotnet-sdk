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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dapr.PluggableComponents;

/// <summary>
/// Represents extension methods for registering Dapr Pluggable Component related services with <see cref="WebApplication"/> instances.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps services needed to host Dapr Pluggable Components via an ASP.NET application.
    /// </summary>
    /// <typeparam name="TBuilder">The type of ASP.NET endpoint builder.</typeparam>
    /// <param name="app">An ASP.NET endpoint builder.</param>
    /// <returns>The current builder instance.</returns>
    public static TBuilder MapDaprPluggableComponentsSupportServices<TBuilder>(this TBuilder app)
        where TBuilder : IEndpointRouteBuilder
    {
        // Dapr component discovery relies on the gRPC reflection service.
        app.MapGrpcReflectionService();

        return app;
    }

    /// <summary>
    /// Maps a gRPC "adaptor" service through which Dapr Pluggable Component calls are made.
    /// </summary>
    /// <typeparam name="T">The type of gRPC service.</typeparam>
    /// <param name="app">An ASP.NET endpoint builder.</param>
    /// <returns>A <see cref="GrpcServiceEndpointConventionBuilder"/> through which the service can be further configured.</returns>
    public static GrpcServiceEndpointConventionBuilder MapDaprPluggableComponentAdaptor<T>(this IEndpointRouteBuilder app)
        where T : class
    {
        return app.MapGrpcService<T>();
    }
}
