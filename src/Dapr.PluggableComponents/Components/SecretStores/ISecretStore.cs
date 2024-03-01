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

namespace Dapr.PluggableComponents.Components.SecretStores;

/// <summary>
/// Represents a secret store Dapr Pluggable Component.
/// </summary>
public interface ISecretStore : IPluggableComponent
{
    /// <summary>
    /// Called to get secret.
    /// </summary>
    /// <param name="request">Properties related to the secret to be retrieved.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, resulting in the retrieved secret, if any.</returns>
    Task<SecretStoreGetResponse> GetAsync(SecretStoreGetRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called to get bulk secret.
    /// </summary>
    /// <param name="request">Properties related to the secret to be retrieved.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, resulting in the retrieved secret, if any.</returns>
    Task<SecretStoreBulkGetResponse> BulkGetAsync(SecretStoreBulkGetRequest request, CancellationToken cancellationToken = default);

}
