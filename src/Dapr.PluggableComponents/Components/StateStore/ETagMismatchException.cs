﻿// ------------------------------------------------------------------------
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

namespace Dapr.PluggableComponents.Components.StateStore;

/// <summary>
/// An exception that represents a possible ETag mismatch.
/// </summary>
/// <remarks>
/// This exception should be thrown for ETag mismatches in the set, bulk set, delete, and bulk delete operations.
/// </remarks>
public sealed class ETagMismatchException : RpcException
{
    private static readonly StatusCode BaseStatusCode = StatusCode.FailedPrecondition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ETagMismatchException"/> class.
    /// </summary>
    public ETagMismatchException()
        : this(Resources.ETagMismatchExceptionMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ETagMismatchException"/> class with a specific error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ETagMismatchException(string message)
        : base(new Status(BaseStatusCode, message), StateStoreErrors.GetETagErrorMetadata(BaseStatusCode, message))
    {
    }
}
