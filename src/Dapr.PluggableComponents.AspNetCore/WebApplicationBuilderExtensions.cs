using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.PluggableComponents;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseDaprPluggableComponents(this WebApplicationBuilder builder, DaprPluggableComponentOptions? options = null)
    {
        string socketExtension = options?.SocketExtension ?? ".sock";
        // TODO: What about Windows?
        string socketDirectory = options?.SocketFolder ?? "/tmp/dapr-components-sockets";
        string socketName = options?.SocketName ?? throw new ArgumentException("Socket name is required.");

        string socketPath = Path.Join(socketDirectory, socketName + socketExtension);

        Directory.CreateDirectory(socketDirectory);

        builder.WebHost.ConfigureKestrel(
            options =>
            {
                // TODO: Address race condition.
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

        // Dapr service discovery relies on gRPC reflection.
        builder.Services.AddGrpcReflection();

        return builder;
    }
}