using System.Collections;
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

        return Task.FromResult(
            new SecretStoreGetResponse
            {
                Secrets = new Dictionary<string, string>
                {
                    { request.Key, Environment.GetEnvironmentVariable(request.Key) ?? String.Empty }
                }
            });
    }

    public Task<SecretStoreBulkGetResponse> BulkGetAsync(SecretStoreBulkGetRequest request, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Get request for all secrets");

        return Task.FromResult(
            new SecretStoreBulkGetResponse
            {
                Keys =
                    Environment
                        .GetEnvironmentVariables()
                        .ToDictionary<string, string>()
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => new SecretStoreGetResponse
                            {
                                Secrets = new Dictionary<string, string>
                                {
                                    { kvp.Key, kvp.Value ?? String.Empty }
                                }
                            })
            });
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion
}

internal static class DictionaryExtensions
{
    public static IEnumerable<KeyValuePair<TKey, TValue?>> ToDictionary<TKey, TValue>(this IDictionary dictionary)
    {
        var enumerator = dictionary.GetEnumerator();

        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<TKey, TValue?>((TKey)enumerator.Key, (TValue?)enumerator.Value);
        }
    }
}
