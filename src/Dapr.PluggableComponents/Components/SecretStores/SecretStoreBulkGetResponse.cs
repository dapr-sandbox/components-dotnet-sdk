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

namespace Dapr.PluggableComponents.Components.SecretStores;

public sealed record SecretStoreBulkGetResponse
{
    public IReadOnlyDictionary<string, string> Data = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static BulkGetSecretResponse ToBulkGetResponse(SecretStoreBulkGetResponse? response)
    {
        var grpcResponse = new BulkGetSecretResponse();
        if (response != null)
        {
            foreach (var item in response.Data)
            {
                var secretItem = new SecretResponse();
                secretItem.Secrets.Add(item.Key, item.Value);
                grpcResponse.Data.Add(item.Key, secretItem);
            }
        }
        return grpcResponse;
    }
}


