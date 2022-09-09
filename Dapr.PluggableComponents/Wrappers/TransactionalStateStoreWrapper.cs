using Dapr.PluggableComponents.Components;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Dapr.PluggableComponents.Services;

public class TransactionalStateStoreWrapper : TransactionalStateStore.TransactionalStateStoreBase
{
    private readonly ILogger<TransactionalStateStoreWrapper> _logger;
    private readonly ITransactionalStateStore _backend;

    public TransactionalStateStoreWrapper(ILogger<TransactionalStateStoreWrapper> logger, ITransactionalStateStore backend)
    {
        this._logger = logger;
        this._backend = backend;
    }

    public override Task<TransactionalStateResponse> Transact(TransactionalStateRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransactionalStateResponse());
    }
}
