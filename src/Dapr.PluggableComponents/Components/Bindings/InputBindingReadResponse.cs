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

namespace Dapr.PluggableComponents.Components.Bindings;

/// <summary>
/// Represents a message being sent from the component to the (Dapr) client.
/// </summary>
/// <remarks>
/// This type is considered a "response" because it is sent by the component in response from the (Dapr) client calling
/// <see cref="IInputBinding.ReadAsync(MessageDeliveryHandler{InputBindingReadRequest, InputBindingReadResponse}, CancellationToken)"/>.
/// </remarks>
public sealed record InputBindingReadResponse
{
    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the metadata associated with the response.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the content type of the message.
    /// </summary>
    public string? ContentType { get; init; }

    internal static ReadResponse ToReadResponse(string messageId, InputBindingReadResponse response)
    {
        var grpcResponse = new ReadResponse
        {
            ContentType = response.ContentType ?? String.Empty,
            Data = ByteString.CopyFrom(response.Data ?? Array.Empty<byte>()),
            MessageId = messageId,
        };

        grpcResponse.Metadata.Add(response.Metadata);

        return grpcResponse;
    }
}
