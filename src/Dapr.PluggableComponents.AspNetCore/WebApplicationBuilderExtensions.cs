using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public static class WebApplicationBuilderExtensions
{
    private static readonly ConcurrentDictionary<string, bool> SocketPaths = new ConcurrentDictionary<string, bool>();

    public static WebApplicationBuilder AddDaprPluggableComponentsSupportServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        // Dapr component discovery relies on the gRPC reflection service.
        builder.Services.AddGrpcReflection();

        return builder;
    }

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
            throw new ArgumentException("No valid socket extension was specified via options or environment variable.", nameof(options));
        }

        if (String.IsNullOrEmpty(socketFolder))
        {
            throw new ArgumentException("No valid socket folder was specified via options or environment variable.", nameof(options));
        }

        if (String.IsNullOrEmpty(socketName))
        {
            throw new ArgumentException("No valid socket name was specified via options or environment variable.", nameof(options));
        }

        string socketPath = Path.Join(socketFolder, socketName + socketExtension);

        if (!SocketPaths.TryAdd(socketPath, true))
        {
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "A service was already added at socket path '{0}'.", socketPath), nameof(options));
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