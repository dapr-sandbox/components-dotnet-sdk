// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.SecretStore;

/// <summary>
/// Represents properties associated with a response to retrieving all secrets from a secret store.
/// </summary>
public sealed record SecretStoreBulkGetResponse
{
    /// <summary>
    /// Gets the groups of secrets.
    /// </summary>
    public IReadOnlyDictionary<string, SecretStoreResponse> Keys { get; init; } = new Dictionary<string, SecretStoreResponse>();

    internal static BulkGetSecretResponse ToBulkGetResponse(SecretStoreBulkGetResponse response)
    {
        BulkGetSecretResponse grpcResponse = new();

        foreach (var item in response.Keys)
        {
            SecretResponse secretResp = new();

            foreach (var sec in item.Value.Data)
            {
                secretResp.Secrets.Add(sec.Key, sec.Value);
            }

            grpcResponse.Data.Add(item.Key, secretResp);
        }

        return grpcResponse;
    }
}

