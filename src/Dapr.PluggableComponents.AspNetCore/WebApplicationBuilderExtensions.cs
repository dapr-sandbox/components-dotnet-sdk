using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddDaprPluggableComponentsServices(this WebApplicationBuilder builder, DaprPluggableComponentsApplicationOptions options)
    {
        string socketExtension = options.SocketExtension
            ?? Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.DaprComponentsSocketsExtension)
            ?? Constants.Defaults.DaprComponentsSocketsExtension;

        // TODO: Add support for native (i.e. non-WSL) Windows.
        string socketFolder = options.SocketFolder
            ?? Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.DaprComponentsSocketsFolder)
            ?? Constants.Defaults.DaprComponentsSocketsFolder;

        string socketName = options.SocketName
            ?? Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.DaprComponentsSocketsName)
            ?? throw new ArgumentException("The socket name was not specified via the options or environment variable.", nameof(options));

        string socketPath = Path.Join(socketFolder, socketName + socketExtension);

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

        builder.Services.AddGrpc();

        // Dapr component discovery relies on the gRPC reflection service.
        builder.Services.AddGrpcReflection();

        return builder;
    }
}