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

namespace Dapr.PluggableComponents.Components.Bindings;

/// <summary>
/// Represents an output binding Dapr Pluggable Component.
/// </summary>
public interface IOutputBinding : IPluggableComponent
{
    /// <summary>
    /// Called to invoke a bound operation.
    /// </summary>
    /// <param name="request">Properties related to the invocation.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, resulting in the invocation response.</returns>
    Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called to return the list of bound operations.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, resulting in the list of bound operation names.</returns>
    Task<string[]> ListOperationsAsync(CancellationToken cancellationToken = default);
}