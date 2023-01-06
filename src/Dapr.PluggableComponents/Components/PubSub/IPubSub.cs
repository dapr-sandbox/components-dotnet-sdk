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

namespace Dapr.PluggableComponents.Components.PubSub;

/// <summary>
/// Represents the handler used to receive acknowledgement of a processed message.
/// </summary>
/// <param name="errorMessage">The error message associated with processing the message, if any.</param>
/// <returns></returns>
public delegate Task MessageAcknowledgementHandler(string? errorMessage);

/// <summary>
/// Represents the handler used to deliver messages to the (Dapr) client.
/// </summary>
/// <param name="response">The response (i.e. message) to deliver to the (Dapr) client.</param>
/// <param name="onAcknowledgement">A handler to receive acknowledgement of the processed message.</param>
/// <returns></returns>
public delegate Task MessageDeliveryHandler(PubSubPullMessagesResponse response, MessageAcknowledgementHandler onAcknowledgement);

/// <summary>
/// Represents a pub-sub Dapr Pluggable Component.
/// </summary>
public interface IPubSub : IPluggableComponent
{
    /// <summary>
    /// Called to publish a message.
    /// </summary>
    /// <param name="request">Properties related to the message being published.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task PublishAsync(PubSubPublishRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Called to establish a stream through which messages can be returned to the (Dapr) client and Dapr acknowledgements returned to the component.
    /// </summary>
    /// <param name="topic">The topic for which to pull messages.</param>
    /// <param name="deliveryHandler">The handler used to deliver messages to the (Dapr) client.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task PullMessagesAsync(PubSubPullMessagesTopic topic, MessageDeliveryHandler deliveryHandler, CancellationToken cancellationToken = default);
}
