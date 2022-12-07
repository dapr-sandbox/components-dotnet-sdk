namespace Dapr.PluggableComponents.Components.PubSub;

public interface IPubSub : IPluggableComponent
{
    Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = default);

    Task PullMessagesAsync(IAsyncEnumerable<PubSubPullMessagesRequest> requests, IAsyncMessageWriter<PubSubPullMessagesResponse> responses, CancellationToken cancellationToken = default);
}
