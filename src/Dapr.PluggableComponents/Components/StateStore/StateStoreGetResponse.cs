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
/// Represents properties associated with a response to retrieving state from a state store.
/// </summary>
public sealed record StateStoreGetResponse
{
    /// <summary>
    /// Gets or sets the key's content type.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to null.
    /// </remarks>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or sets the key's value.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to an empty array.
    /// </remarks>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the ETag used as an If-Match header, to allow certain levels of consistency.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to null.
    /// </remarks>
    public string? ETag { get; init; }

    /// <summary>
    /// Gets or sets the metadata associated with the request.
    /// </summary>
    /// <remarks>
    /// If omitted, defaults to an empty dictionary.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static GetResponse ToGetResponse(StateStoreGetResponse? response)
    {
        var grpcResponse = new GetResponse();

        // NOTE: in case of not found, you should not return any error.

        if (response != null)
        {
            grpcResponse.ContentType = response.ContentType ?? String.Empty;
            grpcResponse.Data = ByteString.CopyFrom(response.Data);
            grpcResponse.Etag = response.ETag != null ? new Etag { Value = response.ETag } : null;

            grpcResponse.Metadata.Add(response.Metadata);
        }

        return grpcResponse;
    }
}
