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
using Dapr.PluggableComponents.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

/// <summary>
/// Represents extension methods for registering Dapr Pluggable Component related services with <see cref="WebApplicationBuilder"/> instances.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    private static readonly ConcurrentDictionary<string, bool> SocketPaths = new();

    /// <summary>
    /// Registers services needed to host Dapr Pluggable Components via an ASP.NET application.
    /// </summary>
    /// <param name="builder">A <see cref="WebApplicationBuilder"/> instance.</param>
    /// <returns>The current <see cref="WebApplicationBuilder"/> instance.</returns>
    public static WebApplicationBuilder AddDaprPluggableComponentsSupportServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        // Dapr component discovery relies on the gRPC reflection service.
        builder.Services.AddGrpcReflection();

        return builder;
    }

    /// <summary>
    /// Registers a Dapr "service" (i.e. a Unix domain socket via which Dapr Pluggable Components can be invoked).
    /// </summary>
    /// <param name="builder">A <see cref="WebApplicationBuilder"/> instance.</param>
    /// <param name="options">Options related to the creation of the socket file.</param>
    /// <returns>The full path to the socketfile representing the service.</returns>
    /// <exception cref="ArgumentException">Thrown if the socket file path cannot be built or has already been registered. </exception>
    public static string AddDaprService(this WebApplicationBuilder builder, DaprPluggableComponentsServiceOptions options)
    {
        string socketExtension = options.SocketExtension
            ?? Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.DaprComponentsSocketsExtension)
            ?? Constants.Defaults.DaprComponentsSocketsExtension;

        // TODO: Add support for native (i.e. non-WSL) Windows.
        string socketFolder = options.SocketFolder
            ?? Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.DaprComponentsSocketsFolder)
            ?? Constants.Defaults.DaprComponentsSocketsFolder;

        string socketName = options.SocketName;

        if (String.IsNullOrEmpty(socketExtension))
        {
            throw new ArgumentException(Resources.WebApplicationBuilderExtensionsNoExtensionMessage, nameof(options));
        }

        if (String.IsNullOrEmpty(socketFolder))
        {
            throw new ArgumentException(Resources.WebApplicationBuilderExtensionsNoFolderMessage, nameof(options));
        }

        if (String.IsNullOrEmpty(socketName))
        {
            throw new ArgumentException(Resources.WebApplicationBuilderExtensionsNoSocketNameMessage, nameof(options));
        }

        string socketPath = Path.Join(socketFolder, socketName + socketExtension);

        if (!SocketPaths.TryAdd(socketPath, true))
        {
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.WebApplicationBuilderExtensionsDuplicateSocketMessage, socketPath), nameof(options));
        }

        Directory.CreateDirectory(socketFolder);

        builder.WebHost.ConfigureKestrel(
            options =>
            {
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                }

                options.ListenUnixSocket(
                    socketPath,
                    listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
            });

        return socketPath;
    }
}
