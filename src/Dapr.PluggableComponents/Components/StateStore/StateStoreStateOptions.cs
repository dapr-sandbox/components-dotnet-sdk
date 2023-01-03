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

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents the concurrency level associated with a state store operation.
/// </summary>
public enum StateStoreConcurrency
{
    /// <summary>
    /// Unspecified.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// First write wins (i.e. optimistic concurrency via ETags).
    /// </summary>
    FirstWrite = 1,

    /// <summary>
    /// Last write wins (i.e. no ETags).
    /// </summary>
    LastWrite = 2
}

/// <summary>
/// Represents the consistency level associated with a state store operation.
/// </summary>
public enum StateStoreConsistency
{
    /// <summary>
    /// Unspecified.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// State stores are eventually consistent.
    /// </summary>
    Eventual = 1,

    /// <summary>
    /// State stores are updated before completing a write request.
    /// </summary>
    Strong = 2
}

/// <summary>
/// Represents options related to state store operation concurrency and consistency.
/// </summary>
public sealed record StateStoreStateOptions
{
    /// <summary>
    /// Gets the concurrency level related to the operation.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="StateStoreConcurrency.Unspecified"/>.
    /// </remarks>
    public StateStoreConcurrency Concurrency { get; init; } = StateStoreConcurrency.Unspecified;

    /// <summary>
    /// Gets the consistency level related to the operation.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="StateStoreConsistency.Unspecified"/>.
    /// </remarks>
    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    internal static StateStoreStateOptions? FromStateOptions(StateOptions options)
    {
        return options != null
            ? new StateStoreStateOptions
            {
                Concurrency = (StateStoreConcurrency)options.Concurrency,
                Consistency = (StateStoreConsistency)options.Consistency
            }
            : null;
    }
}
