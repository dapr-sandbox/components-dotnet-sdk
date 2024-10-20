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

namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentsApplicationTests
{
    private const int TimeoutInMs = 10000;

    [Fact(Timeout = TimeoutInMs)]
    public async Task RunAsync()
    {
        var application = DaprPluggableComponentsApplication.Create();

        var runTask = application.RunAsync();

        var stopAfterDelay =
            async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                await application.StopAsync();
            };

        await Task.WhenAll(runTask, stopAfterDelay());
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task RunAsyncWithCancellation()
    {
        var application = DaprPluggableComponentsApplication.Create();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await application.RunAsync(cts.Token);
    }

    [Fact(Timeout = TimeoutInMs)]
    public async Task Run()
    {
        var application = DaprPluggableComponentsApplication.Create();

        var runTask = Task.Run(
            () =>
            {
                application.Run();
            });

        await Task.Delay(TimeSpan.FromSeconds(1));

        await application.StopAsync();

        await runTask;
    }
}
