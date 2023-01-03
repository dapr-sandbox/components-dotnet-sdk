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

public sealed record StateStoreDeleteRequest(string Key)
{
    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public StateStoreStateOptions? Options { get; init; }

    internal static StateStoreDeleteRequest FromDeleteRequest(DeleteRequest request)
    {
        return new StateStoreDeleteRequest(request.Key)
        {
            ETag = request.Etag?.Value,
            Metadata = request.Metadata,
            Options = StateStoreStateOptions.FromStateOptions(request.Options)
        };
    }
}
