using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class SingletonComponentProvider<T> : IDaprPluggableComponentProvider<T>
{
    private readonly T component;

    public SingletonComponentProvider(T component)
    {
        this.component = component ?? throw new ArgumentNullException(nameof(component));
    }

    #region IDaprPluggableComponentProvider<T> Members

    public T GetComponent(Func<string, Metadata.Entry?> metadataProvider)
    {
        return this.component;
    }

    #endregion
}
