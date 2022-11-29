namespace Dapr.PluggableComponents.Components.PubSub;

public sealed class AckMessageError
{
    public string? Message { get; init; }
}

public sealed class Topic
{
    public string? Name { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

public sealed class PubSubPullMessagesRequest
{
    public string? AckMessageId { get; init; }

    public Topic? Topic { get; init; }

    public AckMessageError? AckError { get; init; }
}