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

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents properties associated with a response to retrieving bulk state from a state store.
/// </summary>
/// <param name="Key">The key associated with the retrieved value.</param>
public sealed record StateStoreBulkStateItem(string Key)
{
    /// <summary>
    /// Gets the key's content type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the key's value.
    /// </summary>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the error message, if retrieval failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the ETag used as an If-Match header, to allow certain levels of consistency.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static BulkStateItem ToBulkStateItem(StateStoreBulkStateItem item)
    {
        var bulkStateItem = new BulkStateItem
        {
            ContentType = item.ContentType ?? String.Empty,
            Data = ByteString.CopyFrom(item.Data),
            Error = item.Error ?? String.Empty,
            Etag = item.ETag != null ? new Etag { Value = item.ETag } : null,
            Key = item.Key
        };

        bulkStateItem.Metadata.Add(item.Metadata);

        return bulkStateItem;
    }
}
