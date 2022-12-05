namespace Dapr.PluggableComponents.Components.PubSub;

public sealed record PubSubPullMessagesTopic(string Name)
{
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
