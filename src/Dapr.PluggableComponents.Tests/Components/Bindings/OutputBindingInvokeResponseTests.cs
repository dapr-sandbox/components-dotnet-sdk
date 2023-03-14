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

using Xunit;

namespace Dapr.PluggableComponents.Components.Bindings;

public sealed class OutputBindingInvokeResponseTests
{
    [Fact]
    public void ToInvokeResponseTests()
    {
        ConversionAssert.ContentTypeEqual(
            contentType => new OutputBindingInvokeResponse { ContentType = contentType },
            response => OutputBindingInvokeResponse.ToInvokeResponse(response),
            response => response.ContentType);

        ConversionAssert.DataEqual(
            data => new OutputBindingInvokeResponse { Data = data },
            response => OutputBindingInvokeResponse.ToInvokeResponse(response),
            response => response.Data.Span.ToArray());

        ConversionAssert.MetadataEqual(
            metadata => new OutputBindingInvokeResponse { Metadata = metadata },
            response => OutputBindingInvokeResponse.ToInvokeResponse(response),
            response => response.Metadata);
    }
}
