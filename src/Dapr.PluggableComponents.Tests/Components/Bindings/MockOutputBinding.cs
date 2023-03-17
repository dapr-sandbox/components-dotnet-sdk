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

namespace Dapr.PluggableComponents.Components.Bindings;

internal interface IMockOutputBinding<T> : IMockPluggableComponent, IOutputBinding where T : class
{
}

internal sealed class MockOutputBinding<T> : IOutputBinding where T : class
{
    private readonly IMockOutputBinding<T> proxy;

    public MockOutputBinding(IMockOutputBinding<T> proxy)
    {
        this.proxy = proxy;

        this.proxy.Create();
    }

    #region IOutputBinding Members

    public Task InitAsync(MetadataRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.InitAsync(request, cancellationToken);
    }

    public Task<OutputBindingInvokeResponse> InvokeAsync(OutputBindingInvokeRequest request, CancellationToken cancellationToken = default)
    {
        return this.proxy.InvokeAsync(request, cancellationToken);
    }

    public Task<string[]> ListOperationsAsync(CancellationToken cancellationToken = default)
    {
        return this.proxy.ListOperationsAsync(cancellationToken);
    }

    #endregion
}
