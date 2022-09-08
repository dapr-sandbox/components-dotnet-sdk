namespace Dapr.PluggableComponents.Components;

public interface ISecretStore
{
    void Init(Dictionary<string,string> properties);
    Dictionary<string,string> GetSecret(string requestName, Dictionary<string,string> metadata);
    void Ping();
}
