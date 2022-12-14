using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public static class WebApplicationBuilderExtensions
{
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

        string socketPath = Path.Join(socketFolder, options.SocketName + socketExtension);

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