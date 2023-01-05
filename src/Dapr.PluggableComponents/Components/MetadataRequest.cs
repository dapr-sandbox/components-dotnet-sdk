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
/// Represents metadata associated with a Dapr Pluggable Component during initialization.
/// </summary>
public sealed record MetadataRequest
{
    /// <summary>
    /// Gets the component configuration metadata properties.
    /// </summary>
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();

    internal static MetadataRequest FromMetadataRequest(Dapr.Client.Autogen.Grpc.v1.MetadataRequest? request)
    {
        var metadataRequest = new MetadataRequest();

        if (request != null)
        {
            metadataRequest = metadataRequest with { Properties = request.Properties };
        }

        return metadataRequest;
    }
}
