namespace Dapr.PluggableComponents.Components;

public struct ConfigurationObject
{
    public List<ConfigurationItem> Items { get; init; }
    public string SubscriptionId { get; init; }
}

public struct ConfigurationItem
{
    public string Key { get; init; }
    public string Data { get; init; }
}

public interface IConfigurationStore
{
    void Init(Dictionary<string,string> properties);
    ConfigurationObject Get(List<string> keys, Dictionary<string, string> metadata);
    bool Unsubscribe(string requestId);
    Queue<ConfigurationObject> Subscribe(List<string> keys, Dictionary<string, string> metadata);
}
