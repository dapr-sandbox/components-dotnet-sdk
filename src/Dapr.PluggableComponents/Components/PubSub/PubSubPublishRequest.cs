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

namespace Dapr.PluggableComponents.Components.PubSub;

/// <summary>
/// Represents properties associated with a message to be published.
/// </summary>
/// <param name="PubSubName">Gets the name of the pub-sub component.</param>
/// <param name="Topic">Gets the topic on which to publish the message.</param>
public sealed record PubSubPublishRequest(string PubSubName, string Topic)
{
    /// <summary>
    /// Gets the message data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; init; } = ReadOnlyMemory<byte>.Empty;

    /// <summary>
    /// Gets the metadata associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the content type of the message.
    /// </summary>
    public string? ContentType { get; init; }

    internal static PubSubPublishRequest FromPublishRequest(PublishRequest request)
    {
        return new PubSubPublishRequest(request.PubsubName, request.Topic)
        {
            ContentType = request.ContentType,
            Data = request.Data.Memory,
            Metadata = request.Metadata
        };
    }
}
