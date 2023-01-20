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

namespace Dapr.PluggableComponents;

/// <summary>
/// Represents options related to the socket file associated with a <see cref="DaprPluggableComponentsApplication"/> service.
/// </summary>
/// <param name="SocketName">Gets or sets the name of the socket.</param>
public sealed record DaprPluggableComponentsServiceOptions(string SocketName)
{
    /// <summary>
    /// Gets or sets the extension added to the socket file name.
    /// </summary>
    /// <remarks>
    /// If omitted and not specified by the <see cref="Constants.EnvironmentVariables.DaprComponentsSocketsExtension"/> environment
    /// variable, the default extension <see cref="Constants.Defaults.DaprComponentsSocketsExtension" /> will be used.
    /// </remarks>
    public string? SocketExtension { get; init; }

    /// <summary>
    /// Gets or sets the folder in which the socket file is created.
    /// </summary>
    /// <remarks>
    /// If omitted and not specified by the <see cref="Constants.EnvironmentVariables.DaprComponentsSocketsFolder"/> environment
    /// variable, the default folder <see cref="Constants.Defaults.DaprComponentsSocketsFolder"/> will be used.
    /// </remarks>
    public string? SocketFolder { get; init; }
}
