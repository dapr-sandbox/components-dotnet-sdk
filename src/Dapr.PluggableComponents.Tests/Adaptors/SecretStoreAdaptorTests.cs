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
using Dapr.PluggableComponents.Adaptors;
using Dapr.PluggableComponents.Components.SecretStore;
using NSubstitute;
using Xunit;

namespace Dapr.PluggableComponents.Tests.Adaptors;

public sealed class SecretStoreAdaptorTests
{
    [Fact]
    public Task InitTests()
    {
        return AdaptorFixture.TestInitAsync(
            () => AdaptorFixture.CreateSecretStore(),
            (fixture, metadataRequest) => fixture.Adaptor.Init(new Proto.Components.V1.SecretStoreInitRequest { Metadata = metadataRequest }, fixture.Context));
    }

    [Fact]
    public Task PingTests()
    {
        return AdaptorFixture.TestPingAsync<SecretStoreAdaptor, ISecretStore>(
            AdaptorFixture.CreateSecretStore,
            fixture => fixture.Adaptor.Ping(new PingRequest(), fixture.Context));
    }

    [Fact]
    public async Task GetSecret()
    {
        using var fixture = AdaptorFixture.CreateSecretStore(Substitute.For<ISecretStore>());

        string key = "key";

        string key1 = "key1";
        string key2 = "key2";

        string value1 = "value1";
        string value2 = "value2";

        fixture.MockComponent
            .GetAsync(Arg.Any<SecretStoreGetRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new SecretStoreGetResponse
                {
                    Secrets = new Dictionary<string, string>
                    {
                        { key1, value1 },
                        { key2, value2 }
                    }
                });

        var getRequest = new Proto.Components.V1.GetSecretRequest();

        getRequest.Key = key;

        var response = await fixture.Adaptor.Get(
            getRequest,
            fixture.Context);

        Assert.Contains(response.Data, item => item.Key == key1 && item.Value == value1);
        Assert.Contains(response.Data, item => item.Key == key2 && item.Value == value2);

        await fixture
            .MockComponent
            .Received(1)
            .GetAsync(Arg.Is<SecretStoreGetRequest>(
                    requests => requests.Key == key),
                Arg.Is<CancellationToken>(cancellationToken => cancellationToken == fixture.Context.CancellationToken));
    }

    [Fact]
    public async Task GetBulkSecrets()
    {
        using var fixture = AdaptorFixture.CreateSecretStore(Substitute.For<ISecretStore>());

        string key = "key";

        string key1 = "key1";
        string key2 = "key2";

        string value1 = "value1";
        string value2 = "value2";

        fixture.MockComponent
            .BulkGetAsync(Arg.Any<SecretStoreBulkGetRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new SecretStoreBulkGetResponse
                {
                    Keys = new Dictionary<string, SecretStoreResponse>
                    {
                        {
                            key,
                            new SecretStoreResponse
                            {
                                Data = new Dictionary<string, string>
                                {
                                    { key1, value1 },
                                    { key2, value2 }
                                }
                            }
                        }
                    }
                });

        var getRequest = new Proto.Components.V1.BulkGetSecretRequest();

        var response = await fixture.Adaptor.BulkGet(
            getRequest,
            fixture.Context);

        Assert.Contains(response.Data, item => item.Key == key);

        var secretResponse = response.Data[key];

        Assert.Contains(secretResponse.Secrets, item => item.Key == key1 && item.Value == value1);
        Assert.Contains(secretResponse.Secrets, item => item.Key == key2 && item.Value == value2);

        await fixture
            .MockComponent
            .Received(1)
            .BulkGetAsync(Arg.Any<SecretStoreBulkGetRequest>(),
                Arg.Is<CancellationToken>(cancellationToken => cancellationToken == fixture.Context.CancellationToken));
    }
}
