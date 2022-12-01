using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class DelegatedComponentProvider<TDelegatingType, TDelegatedType> : IDaprPluggableComponentProvider<TDelegatingType>
    where TDelegatingType : class
{
    private readonly IDaprPluggableComponentProvider<TDelegatedType> provider;

    public DelegatedComponentProvider(IDaprPluggableComponentProvider<TDelegatedType> provider)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    #region IDaprPluggableComponentProvider<TDelegatingType> Members

    public TDelegatingType GetComponent(Func<string, Metadata.Entry?> metadataProvider)
    {
        return (this.provider.GetComponent(metadataProvider) as TDelegatingType)!;
    }

    #endregion
}