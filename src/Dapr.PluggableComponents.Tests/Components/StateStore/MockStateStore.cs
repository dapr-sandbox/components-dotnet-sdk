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

internal interface IMockStateStore<T> : IMockPluggableComponent, IStateStore where T : class
{
}

internal class MockStateStore<T> : IStateStore where T : class
{
    private readonly IMockStateStore<T> proxy;

    public MockStateStore(IMockStateStore<T> proxy)
    {
        this.proxy = proxy;

        this.proxy.Create();
    }

    #region IStateStore Members

    public Task DeleteAsync(StateStoreDeleteRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.DeleteAsync(request, cancellationToken);
    }

    public Task<StateStoreGetResponse?> GetAsync(StateStoreGetRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.GetAsync(request, cancellationToken);
    }

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.InitAsync(request, cancellationToken);
    }

    public Task SetAsync(StateStoreSetRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.SetAsync(request, cancellationToken);
    }

    #endregion
}
