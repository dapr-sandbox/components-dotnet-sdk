using Dapr.PluggableComponents.Components;
using Dapr.PluggableComponents.Components.SecretStore;

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
        this.logger.LogInformation("Get request for secret {key}", request.Key);

        SecretStoreGetResponse? response = null;
        string? data = Environment.GetEnvironmentVariable(request.Key);
        if (data == null)
        {
            data = "";
        }
        Dictionary<string, string> resp = new Dictionary<string, string>();
        resp.Add(request.Key, data);
        response = new SecretStoreGetResponse
        {
            Secrets = resp
        };

        return Task.FromResult(response);
    }

    public Task<SecretStoreBulkGetResponse> BulkGetAsync(SecretStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get request for all secrets");

        SecretStoreBulkGetResponse? response = null;
        SecretStoreResponse? secretStoreResponse = null;
        Dictionary<string, string>? resp = null;
        Dictionary<string, SecretStoreResponse> bulkResp = new Dictionary<string, SecretStoreResponse>();
        foreach (string key in Environment.GetEnvironmentVariables().Keys)
        {
            resp = new Dictionary<string, string>
            {
                {key, Environment.GetEnvironmentVariable(key) ?? String.Empty }
            };
            secretStoreResponse = new SecretStoreResponse
            {
                Data = resp
            };
            bulkResp.Add(key, secretStoreResponse);
        }
        response = new SecretStoreBulkGetResponse
        {
            Keys = bulkResp
        };
        return Task.FromResult(response);
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion
}
