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

namespace Dapr.PluggableComponents.Components.Bindings;

/// <summary>
/// Respresents an acknowledgement of the processing of a message by the (Dapr) client.
/// </summary>
/// <param name="MessageId">Gets the ID of the processed message.</param>
/// <remarks>
/// This type is considered a "request" because it is sent to the component by the (Dapr) client.
/// </remarks>
public sealed record InputBindingReadRequest(string MessageId)
{
    /// <summary>
    /// Gets the message response data, if any.
    /// </summary>
    public ReadOnlyMemory<byte> ResponseData { get; init; } = ReadOnlyMemory<byte>.Empty;

    /// <summary>
    /// Gets the error message associated with the message.
    /// </summary>
    /// <remarks>
    /// If omitted, the message should be considered processed successfully.
    /// </remarks>
    public string? ResponseErrorMessage { get; init; }

    internal static InputBindingReadRequest FromReadRequest(ReadRequest request)
    {
        return new InputBindingReadRequest(request.MessageId)
        {
            ResponseData = request.ResponseData.Memory,
            ResponseErrorMessage = request.ResponseError?.Message
        };
    }
}
