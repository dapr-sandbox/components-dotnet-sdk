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
/// Represents constant values related to Dapr Pluggable Components.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Represents default values for Dapr Pluggable Components.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The default extension for socket files created by Dapr Pluggable Components.
        /// </summary>
        public const string DaprComponentsSocketsExtension = ".sock";

        /// <summary>
        /// The default temp sub-directory in which Dapr Pluggable Components create their socket files.
        /// </summary>
        public const string DaprComponentsSocketsFolder = "dapr-components-sockets";
    }

    /// <summary>
    /// Represents names of environment variables related to Dapr Pluggable Components.
    /// </summary>
    public static class EnvironmentVariables
    {
        /// <summary>
        /// The environment variable name that defines the exension for socket files created by Dapr Pluggable Components.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value <see cref="Constants.Defaults.DaprComponentsSocketsExtension"/> will be used.
        /// </remarks>
        public const string DaprComponentsSocketsExtension = "DAPR_COMPONENTS_SOCKETS_EXTENSION";

        /// <summary>
        /// The environment variable name that defines the folder in which Dapr Pluggable Components create their socket files.
        /// </summary>
        /// <remarks>
        /// If not specified, the default value <see cref="Constants.Defaults.DaprComponentsSocketsFolder"/> will be used.
        /// </remarks>
        public const string DaprComponentsSocketsFolder = "DAPR_COMPONENTS_SOCKETS_FOLDER";
    }
}
