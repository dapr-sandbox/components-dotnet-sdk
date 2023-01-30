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

using Google.Protobuf;
using Google.Rpc;
using Grpc.Core;

namespace Dapr.PluggableComponents.Components.StateStore;

internal static class StateStoreErrors
{
    public static Metadata GetETagErrorMetadata(StatusCode statusCode, string message)
    {
        return GetMetadata(statusCode, GetETagFieldViolation(message));
    }

    public static Metadata GetBulkDeleteRowMismatchErrorMetadata(StatusCode statusCode, int expectedRows, int affectedRows)
    {
        var errorInfo = new Google.Rpc.ErrorInfo();

        errorInfo.Metadata.Add("expected", expectedRows.ToString());
        errorInfo.Metadata.Add("affected", affectedRows.ToString());

        return GetMetadata(statusCode, errorInfo);
    }

    private static BadRequest GetETagFieldViolation(string message)
    {
        var badRequest = new BadRequest();

        badRequest.FieldViolations.Add(
            new Google.Rpc.BadRequest.Types.FieldViolation
            {
                Field = "etag",
                Description = message
            });

        return badRequest;
    }

    private static Metadata GetMetadata(StatusCode baseStatusCode, IMessage message)
    {
        var status = new Google.Rpc.Status
        {
            Code = (int)baseStatusCode
        };

        status.Details.Add(Google.Protobuf.WellKnownTypes.Any.Pack(message));

        var metadata = new Metadata();

        metadata.Add("grpc-status-details-bin", status.ToByteArray());

        return metadata;
    }

}
