using Azure.Storage.Queues;
using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.Bindings;
using Discord;
using Discord.WebSocket;

internal sealed class DiscordBinding : IInputBinding, IOutputBinding, IDisposable
{
    private readonly ILogger<DiscordBinding> logger;

    private string? token;
    private DiscordSocketClient? client;
    private readonly SemaphoreSlim clientLock = new SemaphoreSlim(1);

    public DiscordBinding(ILogger<DiscordBinding> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IInputBinding Members

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Properties.TryGetValue("token", out string? token))
        {
            this.token = token;
        }

        if (this.token == null)
        {
            throw new InvalidOperationException("The \"token\" property must be set.");
        }

        return Task.CompletedTask;
    }

    public async Task ReadAsync(MessageDeliveryHandler<InputBindingReadRequest, InputBindingReadResponse> deliveryHandler, CancellationToken cancellationToken = default)
    {
        var client = await this.GetClient(cancellationToken);

        Func<SocketMessage, Task> messageReceivedHandler =
            async message =>
            {
                var discordMessage = new DiscordMessage(
                    message.Channel.Id.ToString(),
                    message.Content);

                await deliveryHandler(
                    new InputBindingReadResponse
                    {
                        ContentType = "application/json",
                        Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(discordMessage))
                    });
            };

        client.MessageReceived += messageReceivedHandler;

        try
        {
            await Task.Delay(-1, cancellationToken);
        }
        finally
        {
            client.MessageReceived -= messageReceivedHandler;
        }
    }

    #endregion

    #region IOutputBinding Members

    public async Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default)
    {
        switch (request.Operation)
        {
            case "SendMessage":

                string messageString = Encoding.UTF8.GetString(request.Data.Span);
                var message = JsonSerializer.Deserialize<DiscordMessage>(messageString);

                if (message == null)
                {
                    throw new InvalidOperationException("Data was not a valid Discord message.");
                }

                ulong channelId = UInt64.Parse(message.ChannelId);

                var client = await GetClient(cancellationToken);

                var channel = await client.GetChannelAsync(channelId);
                var textChannel = channel as SocketTextChannel;

                if (textChannel == null)
                {
                    throw new InvalidOperationException("Unable to obtain the channel.");
                }

                await textChannel.SendMessageAsync(message.Content);

                return new OutputBindingInvokeResponse();

            default:

                throw new InvalidOperationException($"The operation '{request.Operation}' is not supported.");
        }
    }

    public Task<string[]> ListOperationsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new[]
            {
                "SendMessage"
            });
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        this.clientLock.Dispose();
        this.client?.Dispose();
    }

    #endregion

    private async Task<DiscordSocketClient> GetClient(CancellationToken cancellationToken)
    {
        if (this.client == null)
        {
            await this.clientLock.WaitAsync(cancellationToken);

            try
            {
                if (this.client == null)
                {
                    var config = new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                    };

                    this.client = new DiscordSocketClient(config);

                    client.Log +=
                        message =>
                        {
                            this.logger.LogInformation(message.Message);

                            return Task.CompletedTask;
                        };

                    await client.LoginAsync(TokenType.Bot, this.token);

                    await client.StartAsync();
                }
            }
            finally
            {
                this.clientLock.Release();
            }
        }

        return this.client;
    }

    private sealed record DiscordMessage(
        [property: JsonPropertyName("channelId")] string ChannelId,
        [property: JsonPropertyName("content")] string Content
    );
}
