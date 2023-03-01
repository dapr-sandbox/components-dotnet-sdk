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

using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

internal sealed class TestServerCallContext : ServerCallContext, IDisposable
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();

    #region ServerCallContext Overrides

    protected override string MethodCore => throw new NotImplementedException();

    protected override string HostCore => throw new NotImplementedException();

    protected override string PeerCore => throw new NotImplementedException();

    protected override DateTime DeadlineCore => throw new NotImplementedException();

    protected override Metadata RequestHeadersCore => throw new NotImplementedException();

    protected override CancellationToken CancellationTokenCore => this.cts.Token;

    protected override Metadata ResponseTrailersCore => throw new NotImplementedException();

    protected override Status StatusCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected override WriteOptions? WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected override AuthContext AuthContextCore => throw new NotImplementedException();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
    {
        throw new NotImplementedException();
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        this.cts.Dispose();
    }

    #endregion
}
