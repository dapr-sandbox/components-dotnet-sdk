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

public enum StateStoreConcurrency
{
    Unspecified = 0,
    FirstWrite = 1,
    LastWrite = 2
}

public enum StateStoreConsistency
{
    Unspecified = 0,
    Eventual = 1,
    Strong = 2
}

public sealed record StateStoreStateOptions
{
    public StateStoreConcurrency Concurrency { get; init; } = StateStoreConcurrency.Unspecified;

    public StateStoreConsistency Consistency { get; init; } = StateStoreConsistency.Unspecified;

    public static StateStoreStateOptions? FromStateOptions(StateOptions options)
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
