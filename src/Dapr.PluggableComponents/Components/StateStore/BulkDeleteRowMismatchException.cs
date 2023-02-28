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

using System.Globalization;
using Grpc.Core;

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// An exception that represents an occurrence of a bulk deletion not effecting the expected number of rows.
/// </summary>
/// <remarks>
/// This exception should be thrown only from bulk delete operations.
/// </remarks>
public sealed class BulkDeleteRowMismatchException : RpcException
{
    private const StatusCode BaseStatusCode = StatusCode.Internal;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkDeleteRowMismatchException"/> class.
    /// </summary>
    /// <param name="expectedRows">The number of rows expected to be affected.</param>
    /// <param name="affectedRows">The number of rows actually affected.</param>
    public BulkDeleteRowMismatchException(int expectedRows, int affectedRows)
        : this(String.Format(CultureInfo.CurrentCulture, "Delete affected {0} rows, but expected {1}.", affectedRows, expectedRows), expectedRows, affectedRows)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BulkDeleteRowMismatchException"/> class with a specific error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="expectedRows">The number of rows expected to be affected.</param>
    /// <param name="affectedRows">The number of rows actually affected.</param>
    public BulkDeleteRowMismatchException(string message, int expectedRows, int affectedRows)
        : base(new Status(BaseStatusCode, message), StateStoreErrors.GetBulkDeleteRowMismatchErrorMetadata(BaseStatusCode, expectedRows, affectedRows))
    {
    }
}
