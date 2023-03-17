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

using System.Threading.Channels;
using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

internal sealed class AsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
    where TRequest : class
{
    private readonly Channel<TRequest> channel = Channel.CreateUnbounded<TRequest>();
    private TRequest? current;

    public ValueTask AddAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        return this.channel.Writer.WriteAsync(request, cancellationToken);
    }

    public void Complete()
    {
        this.channel.Writer.Complete();
    }

    #region IAsyncStreamReader<PullMessagesRequest> Members

    public TRequest Current => this.current ?? throw new InvalidOperationException();

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        var result = await this.channel.Reader.WaitToReadAsync(cancellationToken);

        if (result && this.channel.Reader.TryRead(out this.current))
        {
            return true;
        }

        this.current = null;

        return false;
    }

    #endregion
}
