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

namespace Dapr.PluggableComponents.Components.PubSub;

/// <summary>
/// Represents a message being "pulled" from the source and delivered to the (Dapr) client.
/// </summary>
/// <param name="TopicName">Gets the name of the topic on which the message was published.</param>
/// <remarks>
/// This type is considered a "response" because it is sent by the component in response to the (Dapr) client calling
/// <see cref="IPubSub.PullMessagesAsync(Dapr.PluggableComponents.Components.PubSub.PubSubPullMessagesTopic, Dapr.PluggableComponents.Components.PubSub.MessageDeliveryHandler, CancellationToken)" />.
/// </remarks>
public sealed record PubSubPullMessagesResponse(string TopicName)
{
    /// <summary>
    /// Gets the message data.
    /// </summary>
    public byte[] Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the content type of the message.
    /// </summary>
    public string? ContentType { get; init; }

    internal static PullMessagesResponse ToPullMessagesResponse(string messageId, PubSubPullMessagesResponse message)
    {
        var grpcResponse = new PullMessagesResponse
        {
            ContentType = message.ContentType ?? String.Empty,
            Data = message.Data != null ? ByteString.CopyFrom(message.Data) : ByteString.Empty,
            Id = messageId,
            TopicName = message.TopicName
        };

        grpcResponse.Metadata.Add(message.Metadata);

        return grpcResponse;
    }
}
