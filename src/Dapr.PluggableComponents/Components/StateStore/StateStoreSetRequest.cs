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
/// Represents properties associated with a request to set or change state in a state store.
/// </summary>
/// <param name="Key">The key that should be set.</param>
/// <param name="Value">The value to which the key should be set.</param>
public sealed record StateStoreSetRequest(string Key, ReadOnlyMemory<byte> Value)
{
    /// <summary>
    /// Gets the key's content type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the ETag used as an If-Match header, to allow certain levels of consistency.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets options related to the change.
    /// </summary>
    public StateStoreStateOptions? Options { get; init; }

    internal static StateStoreSetRequest FromSetRequest(SetRequest request)
    {
        return new StateStoreSetRequest(request.Key, request.Value.Memory)
        {
            ContentType = request.ContentType,
            ETag = request.Etag?.Value,
            Metadata = request.Metadata,
            Options = StateStoreStateOptions.FromStateOptions(request.Options)
        };
    }
}
