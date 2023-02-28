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

internal interface IMockBulkStateStore<T> : IMockStateStore<T>, IBulkStateStore where T : class
{
}

internal sealed class MockBulkStateStore<T> : MockStateStore<T>, IBulkStateStore where T : class
{
    private readonly IMockBulkStateStore<T> proxy;

    public MockBulkStateStore(IMockBulkStateStore<T> proxy)
        : base(proxy)
    {
        this.proxy = proxy;
    }

    #region IBulkStateStore Members

    public Task BulkDeleteAsync(StateStoreDeleteRequest[] requests, CancellationToken cancellationToken = default)
    {
        return this.proxy.BulkDeleteAsync(requests, cancellationToken);
    }

    public Task<StateStoreBulkStateItem[]> BulkGetAsync(StateStoreGetRequest[] requests, CancellationToken cancellationToken = default)
    {
        return this.proxy.BulkGetAsync(requests, cancellationToken);
    }

    public Task BulkSetAsync(StateStoreSetRequest[] requests, CancellationToken cancellationToken = default)
    {
        return this.proxy.BulkSetAsync(requests, cancellationToken);
    }

    #endregion
}
