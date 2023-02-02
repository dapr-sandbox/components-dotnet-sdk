using Azure.Storage.Queues;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.PubSub;

internal sealed class AzureStorageQueuesPubSub : IPubSub
{
    private readonly ILogger<AzureStorageQueuesPubSub> logger;

    private string? connectionString;
    private string? queueName;
    private TimeSpan pollInterval = TimeSpan.FromSeconds(5);
    private int maxMessages = 5; 

    public AzureStorageQueuesPubSub(ILogger<AzureStorageQueuesPubSub> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IPubSub Members

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Init request");

        this.connectionString = request.Properties["connectionString"];
        this.queueName = request.Properties["queueName"];

        if (request.Properties.TryGetValue("pollIntervalSeconds", out var pollIntervalString))
        {
            this.pollInterval = TimeSpan.FromSeconds(Int32.Parse(pollIntervalString));
        }

        if (request.Properties.TryGetValue("maxMessages", out var maxMessagesString))
        {
            this.maxMessages = Int32.Parse(maxMessagesString);
        }

        return Task.CompletedTask;
    }

    public async Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Publish request");

        var queueClient = new QueueClient(this.connectionString, this.queueName);

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        await queueClient.SendMessageAsync(Encoding.UTF8.GetString(request.Data.Span), cancellationToken);
    }

    public async Task PullMessagesAsync(PubSubPullMessagesTopic topic, MessageDeliveryHandler<string?, PubSubPullMessagesResponse> handler, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Pull messages request for topic \"{0}\"", topic.Name);

        var queueClient = new QueueClient(this.connectionString, this.queueName);

        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await queueClient.ReceiveMessagesAsync(this.maxMessages, cancellationToken: cancellationToken);

            this.logger.LogInformation("Received messages: {0}", response.Value.Length);

            foreach (var message in response.Value)
            {
                var cloudEventBytes = message.Body.ToArray();
                var cloudEvent = JsonSerializer.Deserialize<CloudEvent>(Encoding.UTF8.GetString(cloudEventBytes));

                if (cloudEvent != null && cloudEvent.Topic != null)
                {
                    this.logger.LogInformation("Delivering message {0}.", cloudEvent.Id);

                    await handler(
                        new PubSubPullMessagesResponse(cloudEvent.Topic)
                        {
                            ContentType = cloudEvent.DataContentType,
                            Data = cloudEventBytes
                        },
                        async errorMessage =>
                        {
                            this.logger.LogInformation("Acknowledgement for message {0} received with error: {1}", cloudEvent.Id, errorMessage);

                            if (String.IsNullOrEmpty(errorMessage))
                            {
                                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
                            }
                        });
                }
            }

            await Task.Delay(this.pollInterval, cancellationToken);
        }
    }

    #endregion

    private sealed class CloudEvent
    {
        [JsonPropertyName("data")]
        public string? Data { get; init; }

        [JsonPropertyName("datacontenttype")]
        public string? DataContentType { get; init; }

        [JsonPropertyName("id")]
        public string? Id { get; init;}

        [JsonPropertyName("pubsubname")]
        public string? PubSubName { get; init; }

        [JsonPropertyName("source")]
        public string? Source { get; init; }

        [JsonPropertyName("specversion")]
        public string? SpecVersion { get; init; }

        [JsonPropertyName("time")]
        public string? Time { get; init; }

        [JsonPropertyName("topic")]
        public string? Topic { get; init; }

        [JsonPropertyName("traceid")]
        public string? TraceId { get; init; }

        [JsonPropertyName("traceparent")]
        public string? TraceParent { get; init; }

        [JsonPropertyName("tracestate")]
        public string? TraceState { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}