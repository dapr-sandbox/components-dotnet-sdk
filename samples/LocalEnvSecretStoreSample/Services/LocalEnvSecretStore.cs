using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.SecretStores;

namespace LocalEnvSecretStoreSample.Services;

internal sealed class LocalEnvSecretStore : ISecretStore
{
    private readonly ILogger<LocalEnvSecretStore> logger;

    public LocalEnvSecretStore(ILogger<LocalEnvSecretStore> logger)
    {
        this.logger = logger;
    }

    #region ISecretStore Members

    public Task<SecretStoreGetResponse> GetAsync(SecretStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get request for secret {key}", request.secretName);

        SecretStoreGetResponse? response = null;
        string data = Environment.GetEnvironmentVariable(request.secretName);
        if (data == null)
        {
            data = "";
        }
        Dictionary<string, string> resp = new Dictionary<string, string>();
        resp.Add(request.secretName, data);
        response = new SecretStoreGetResponse
        {
            Data = resp
        };

        return Task.FromResult(response);
    }

    public Task<SecretStoreBulkGetResponse> BulkGetAsync(SecretStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get request for all secrets");

        SecretStoreBulkGetResponse? response = null;
        //this.logger.LogInformation(Environment.GetEnvironmentVariables());
        

        return Task.FromResult(response);
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion
}
