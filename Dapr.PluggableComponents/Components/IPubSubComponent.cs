namespace Dapr.PluggableComponents.Components;

public struct PubSubMessage {
    internal string Topic { get; init; }
    internal byte[] Data { get; init; }
}

public interface IPubSubComponent
{
    void Init(Dictionary<string,string> properties);
    void Publish(string topic, PubSubMessage msg);
    Queue<PubSubMessage> Subscribe(string topic);
    List<string> Features();
}
