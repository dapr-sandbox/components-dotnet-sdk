using System.Collections.Concurrent;
using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class MultiplexedComponentProvider<T> : IDaprPluggableComponentProvider<T>
{
    private const string metadataInstanceId = "x-component-instance";

    private readonly Func<IServiceProvider, string?, T> componentProvider;
    private readonly ConcurrentDictionary<string, Lazy<T>> components = new ConcurrentDictionary<string, Lazy<T>>();
    private readonly Lazy<T> defaultComponent;
    private readonly IServiceProvider serviceProvider;

    public MultiplexedComponentProvider(IServiceProvider serviceProvider, Func<IServiceProvider, string?, T> componentProvider)
    {
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        this.defaultComponent = new Lazy<T>(() => componentProvider(this.serviceProvider, null));
    }

    #region IDaprPluggableComponentProvider<T> Members

    public T GetComponent(ServerCallContext context)
    {
        var entry = context.RequestHeaders.Get(metadataInstanceId);

        var component =
            entry != null
                ? this.components.GetOrAdd(entry.Value, instanceId => new Lazy<T>(() => this.componentProvider(this.serviceProvider, instanceId)))
                : this.defaultComponent;

        return component.Value;
    }

    #endregion
}
