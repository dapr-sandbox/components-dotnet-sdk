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
using Google.Protobuf;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// Represents an individual item returned by a query of a state store.
/// </summary>
/// <param name="Key">The state store key associated with the item.</param>
public sealed record StateStoreQueryItem(string Key)
{
    /// <summary>
    /// Gets or sets the key's content type.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to null.
    /// </remarks>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or sets the item's value.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to an empty array.
    /// </remarks>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets an error message associated with the query.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to null.
    /// </remarks>
    public string? Error { get; init; }

    /// <summary>
    /// Gets or sets the ETag used as an If-Match header, to allow certain levels of consistency.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to null.
    /// </remarks>
    public string? ETag { get; init; }

    internal static QueryItem ToQueryItem(StateStoreQueryItem item)
        => new QueryItem
        {
            ContentType = item.ContentType ?? String.Empty,
            Data = ByteString.CopyFrom(item.Data ?? Array.Empty<byte>()),
            Error = item.Error ?? String.Empty,
            Etag = item.ETag != null ? new Etag { Value = item.ETag } : null,
            Key = item.Key
        };
}
