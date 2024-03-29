﻿// ------------------------------------------------------------------------
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
/// Represents a Dapr Pluggable Component that can respond to liveness checks.
/// </summary>
/// <remarks>
/// This interface is optional. If not implemented, the component is always considered "live".
/// </remarks>
public interface IPluggableComponentLiveness
{
    /// <summary>
    /// Called to determine the "liveness" of this component.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. A task that results in
    /// <see cref="TaskStatus.RanToCompletion"/> indicates this component is "live". Implementors should
    /// return a task that results in <see cref="TaskStatus.Faulted"/> to indicate this component is not
    /// healthy.</returns>
    Task PingAsync(CancellationToken cancellationToken = default);
}
