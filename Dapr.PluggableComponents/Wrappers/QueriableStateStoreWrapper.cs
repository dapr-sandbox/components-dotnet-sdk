using Dapr.PluggableComponents.Components;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Dapr.PluggableComponents.Services;

public class QueriableStateStoreWrapper : QueriableStateStore.QueriableStateStoreBase
{
    private readonly ILogger<QueriableStateStoreWrapper> _logger;
    private readonly IQueriableStateStore _backend;

    public QueriableStateStoreWrapper(ILogger<QueriableStateStoreWrapper> logger, IQueriableStateStore backend)
    {
        this._logger = logger;
        this._backend = backend;
    }

    public override Task<QueryResponse> Query(QueryRequest request, ServerCallContext context)
    {
        return Task.FromResult(new QueryResponse());
    }
}
