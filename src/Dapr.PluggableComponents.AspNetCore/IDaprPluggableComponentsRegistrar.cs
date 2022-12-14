namespace Dapr.PluggableComponents;

internal interface IDaprPluggableComponentsRegistrar
{
    void RegisterAdaptor<TAdaptor>() where TAdaptor : class;

    void RegisterComponent<TComponent>(string socketPath) where TComponent : class;

    void RegisterComponent<TComponent>(string socketPath, Func<IServiceProvider, string?, TComponent> componentFactory) where TComponent : class;

    void RegisterProvider<TComponent, TComponentImpl>(string socketPath)
        where TComponent : class
        where TComponentImpl : class;
}