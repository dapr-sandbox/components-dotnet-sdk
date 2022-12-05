namespace Dapr.PluggableComponents.Components.PubSub;

public sealed record PubSubPullMessagesRequest
{
    public string? AckMessageId { get; init; }

    public PubSubPullMessagesTopic? Topic { get; init; }

    public string? AckMessageError { get; init; }
}