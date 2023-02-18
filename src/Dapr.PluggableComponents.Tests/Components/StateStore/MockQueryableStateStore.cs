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

namespace Dapr.PluggableComponents.Components.StateStore;

internal interface IMockQueryableStateStore<T> : IMockStateStore<T>, IQueryableStateStore where T : class
{
}

internal sealed class MockQueryableStateStore<T> : MockStateStore<T>, IQueryableStateStore where T : class
{
    private readonly IMockQueryableStateStore<T> proxy;

    public MockQueryableStateStore(IMockQueryableStateStore<T> proxy)
        : base(proxy)
    {
        this.proxy = proxy;
    }

    #region IQueryableMockStateStore Members

    public Task<StateStoreQueryResponse> QueryAsync(StateStoreQueryRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.QueryAsync(request, cancellationToken);
    }

    #endregion
}
