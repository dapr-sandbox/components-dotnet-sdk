using Dapr.PluggableComponents.Components;
using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class ServerStreamWriterAdaptor<TServerType, TLocalType> : IAsyncMessageWriter<TLocalType>
{
    private readonly Func<TLocalType, TServerType> transform;
    private readonly IServerStreamWriter<TServerType> writer;

    public ServerStreamWriterAdaptor(IServerStreamWriter<TServerType> writer, Func<TLocalType, TServerType> transform)
    {
        this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    #region IAsyncMessageWriter<T> Members

    public Task WriteAsync(TLocalType message, CancellationToken cancellationToken = default)
    {
        return this.writer.WriteAsync(this.transform(message), cancellationToken);
    }

    #endregion
}