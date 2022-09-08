namespace Dapr.PluggableComponents.Components;



public interface INameResolver
{
    String? Lookup(string key);
    void Init(Dictionary<string,string> properties);
}
