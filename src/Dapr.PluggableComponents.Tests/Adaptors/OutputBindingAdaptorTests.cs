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

using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components.Bindings;
using Dapr.Proto.Components.V1;
using NSubstitute;
using Xunit;

namespace Dapr.PluggableComponents.Adaptors;

public sealed class OutputBindignAdaptorTests
{
    [Fact]
    public Task InitTests()
    {
        return AdaptorFixture.TestInitAsync(
            () => AdaptorFixture.CreateOutputBinding(),
            (fixture, metadataRequest) => fixture.Adaptor.Init(new Proto.Components.V1.OutputBindingInitRequest { Metadata = metadataRequest }, fixture.Context));
    }

    [Fact]
    public Task PingTests()
    {
        return AdaptorFixture.TestPingAsync<OutputBindingAdaptor, IOutputBinding>(
            AdaptorFixture.CreateOutputBinding,
            fixture => fixture.Adaptor.Ping(new PingRequest(), fixture.Context));
    }

    [Fact]
    public async Task InvokeTests()
    {
        using var fixture = AdaptorFixture.CreateOutputBinding();

        string operation = "operation";
        string contentType = "application/json";

        var request = new InvokeRequest { Operation = operation };

        fixture.MockComponent
            .InvokeAsync(Arg.Is<OutputBindingInvokeRequest>(request => request.Operation == operation), Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken))
            .Returns(new OutputBindingInvokeResponse { ContentType = contentType });

        var response = await fixture.Adaptor.Invoke(new InvokeRequest { Operation = operation }, fixture.Context);

        Assert.Equal(contentType, response.ContentType);

        await fixture.MockComponent
            .Received(1)
            .InvokeAsync(Arg.Is<OutputBindingInvokeRequest>(request => request.Operation == operation), Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }

    [Fact]
    public async Task ListOperationsTests()
    {
        using var fixture = AdaptorFixture.CreateOutputBinding();

        var operations = new[] { "operation1", "operation2" };

        fixture.MockComponent
            .ListOperationsAsync(Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken))
            .Returns(operations);

        var response = await fixture.Adaptor.ListOperations(new ListOperationsRequest(), fixture.Context);

        Assert.Equal(operations, response.Operations);

        await fixture.MockComponent
            .Received(1)
            .ListOperationsAsync(Arg.Is<CancellationToken>(token => token == fixture.Context.CancellationToken));
    }
}
