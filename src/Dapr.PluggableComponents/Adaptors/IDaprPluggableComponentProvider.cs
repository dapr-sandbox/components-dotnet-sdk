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

using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

/// <summary>
/// Represents a means of retrieving the Dapr Pluggable Component instance associated with a given gRPC request.
/// </summary>
/// <typeparam name="T">The type of Dapr Pluggable Component being retrieved.</typeparam>
/// <remarks>
/// As the gRPC protocol adaptors are scoped to requests (i.e. a new instance created for each request), while
/// Dapr Pluggable Components may *not* be, the adaptors use a provider-based mechanism to obtain the component
/// associated with a given request.
/// </remarks>
public interface IDaprPluggableComponentProvider<out T>
{
    /// <summary>
    /// Gets the Dapr Pluggable Component instance associated with a given gRPC request.
    /// </summary>
    /// <param name="context">The gRPC request context.</param>
    /// <returns>The component instance associated with the request.</returns>
    T GetComponent(ServerCallContext context);
}
