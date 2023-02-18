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

using Dapr.Proto.Components.V1;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreStateOptionsTests
{
    [Fact]
    public void FromStateOptionsTests()
    {
        StateOptions? grpcOptions = null;

        var options = StateStoreStateOptions.FromStateOptions(grpcOptions);

        Assert.Null(options);

        grpcOptions = new StateOptions
        {
            Concurrency = StateOptions.Types.StateConcurrency.ConcurrencyFirstWrite,
            Consistency = StateOptions.Types.StateConsistency.ConsistencyEventual
        };

        options = StateStoreStateOptions.FromStateOptions(grpcOptions);

        Assert.NotNull(options);

        Assert.Equal(StateStoreConcurrency.FirstWrite, options.Concurrency);
        Assert.Equal(StateStoreConsistency.Eventual, options.Consistency);
    }

    [Theory]
    [InlineData(StateOptions.Types.StateConcurrency.ConcurrencyFirstWrite, StateStoreConcurrency.FirstWrite)]
    [InlineData(StateOptions.Types.StateConcurrency.ConcurrencyLastWrite, StateStoreConcurrency.LastWrite)]
    [InlineData(StateOptions.Types.StateConcurrency.ConcurrencyUnspecified, StateStoreConcurrency.Unspecified)]
    public void FromStateOptionsConcurrencyTests(StateOptions.Types.StateConcurrency grpcConcurrency, StateStoreConcurrency expectedConcurrency)
    {
        var grpcOptions = new StateOptions
        {
            Concurrency = grpcConcurrency
        };

        var options = StateStoreStateOptions.FromStateOptions(grpcOptions);

        Assert.NotNull(options);
        Assert.Equal(expectedConcurrency, options.Concurrency);
    }

    [Theory]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyEventual, StateStoreConsistency.Eventual)]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyStrong, StateStoreConsistency.Strong)]
    [InlineData(StateOptions.Types.StateConsistency.ConsistencyUnspecified, StateStoreConsistency.Unspecified)]
    public void FromStateOptionsConsistencyTests(StateOptions.Types.StateConsistency grpcConsistency, StateStoreConsistency expectedConsistency)
    {
        var grpcOptions = new StateOptions
        {
            Consistency = grpcConsistency
        };

        var options = StateStoreStateOptions.FromStateOptions(grpcOptions);

        Assert.NotNull(options);
        Assert.Equal(expectedConsistency, options.Consistency);
    }
}