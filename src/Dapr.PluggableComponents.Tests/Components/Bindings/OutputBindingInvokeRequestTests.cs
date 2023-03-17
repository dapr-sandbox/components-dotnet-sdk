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

using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Xunit;

namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class OutputBindingInvokeRequestTests
{
    [Fact]
    public void FromInvokeRequestTests()
    {
        ConversionAssert.DataEqual(
            data => new InvokeRequest { Data = ByteString.CopyFrom(data) },
            OutputBindingInvokeRequest.FromInvokeRequest,
            request => request.Data.Span.ToArray());

        ConversionAssert.MetadataEqual(
            metadata =>
            {
                var request = new InvokeRequest();

                request.Metadata.Add(metadata);

                return request;
            },
            OutputBindingInvokeRequest.FromInvokeRequest,
            request => request.Metadata);

        ConversionAssert.StringEqual(
            operation => new InvokeRequest { Operation = operation },
            OutputBindingInvokeRequest.FromInvokeRequest,
            request => request.Operation);
    }
}
