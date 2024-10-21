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

using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Components.SecretStore;

/// <summary>
/// Represents properties associated with a response to retrieving secret from a secret store.
/// </summary>
public sealed record SecretStoreGetResponse
{
    /// <summary>
    /// Gets the secrets.
    /// </summary>
    public IReadOnlyDictionary<string, string> Secrets { get; init; } = new Dictionary<string, string>();

    internal static GetSecretResponse ToGetResponse(SecretStoreGetResponse response)
    {
        var grpcResponse = new GetSecretResponse();

        // NOTE: in case of not found, you should not return any error.

        if (response != null)
        {
            grpcResponse.Data.Add(response.Secrets);
        }

        return grpcResponse;
    }
}
