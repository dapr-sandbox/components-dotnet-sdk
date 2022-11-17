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

namespace Dapr.PluggableComponents.Components;

/// <summary>
/// Represents the root interface of all Dapr Pluggable Components.
/// </summary>
public interface IPluggableComponent
{
    /// <summary>
    /// Initializes the Dapr Pluggable Component.
    /// </summary>
    /// <param name="request">Metadata associated with this component.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>This method will be invoked once per configured Dapr component.</remarks>
    Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default);
}
