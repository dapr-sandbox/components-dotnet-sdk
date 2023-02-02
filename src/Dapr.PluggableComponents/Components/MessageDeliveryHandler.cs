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

namespace Dapr.PluggableComponents.Components;

/// <summary>
/// Represents the handler used to receive acknowledgement of a processed message.
/// </summary>
/// <param name="request">The request associated with processing the message, if any.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task MessageAcknowledgementHandler<TRequest>(TRequest request);

/// <summary>
/// Represents the handler used to deliver messages to the (Dapr) client.
/// </summary>
/// <param name="response">The response (i.e. message) to deliver to the (Dapr) client.</param>
/// <param name="onAcknowledgement">A handler to receive acknowledgement of the processed message.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task MessageDeliveryHandler<TRequest, TResponse>(TResponse response, MessageAcknowledgementHandler<TRequest>? onAcknowledgement = null);
