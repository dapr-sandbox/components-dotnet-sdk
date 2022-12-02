namespace Dapr.PluggableComponents.Components.PubSub;

public interface IPubSub : IPluggableComponent
{
    Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = default);

    Task PullMessagesAsync(IAsyncEnumerable<PubSubPullMessagesRequest> request, IAsyncMessageWriter<PubSubPullMessagesResponse> response, CancellationToken cancellationToken = default);
}
