using System.Collections.Concurrent;
using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class MultiplexedComponentProvider<T> : IDaprPluggableComponentProvider<T>
{
    private const string metadataInstanceId = "x-component-instance";

    private readonly Func<string?, T> componentProvider;
    private readonly ConcurrentDictionary<string, T> components = new ConcurrentDictionary<string, T>();
    private readonly Lazy<T> defaultComponent;

    public MultiplexedComponentProvider(Func<string?, T> componentProvider)
    {
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));

        this.defaultComponent = new Lazy<T>(() => componentProvider(null));
    }

    #region IDaprPluggableComponentProvider<T> Members

    public T GetComponent(Func<string, Metadata.Entry?> metadataProvider)
    {
        var entry = metadataProvider(metadataInstanceId);

        var component =
            entry != null
                ? this.components.GetOrAdd(entry.Value, this.componentProvider)
                : this.defaultComponent.Value;

        return component;
    }

    #endregion
}
