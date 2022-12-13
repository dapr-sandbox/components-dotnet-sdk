using Dapr.PluggableComponents.Components.StateStore;
using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Dapr.Proto.Components.V1.TransactionalStateStore;

namespace Dapr.PluggableComponents.Adaptors;

public class TransactionalStateStoreAdaptor : TransactionalStateStoreBase
{
    private readonly ILogger<TransactionalStateStoreAdaptor> logger;
    private readonly IDaprPluggableComponentProvider<ITransactionalStateStore> componentProvider;

    public TransactionalStateStoreAdaptor(ILogger<TransactionalStateStoreAdaptor> logger, IDaprPluggableComponentProvider<ITransactionalStateStore> componentProvider)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentProvider = componentProvider ?? throw new ArgumentNullException(nameof(componentProvider));
    }

    public override async Task<TransactionalStateResponse> Transact(TransactionalStateRequest request, ServerCallContext context)
    {
        await this.GetStateStore(context).TransactAsync(
            new StateStoreTransactRequest
            {
                Metadata = request.Metadata,
                Operations = request.Operations.Select(ToOperation).WhereNonNull().ToArray()
            },
            context.CancellationToken);

        return new TransactionalStateResponse();
    }

    private ITransactionalStateStore GetStateStore(ServerCallContext context)
    {
        return this.componentProvider.GetComponent(context);
    }

    public static StateStoreTransactOperation? ToOperation(TransactionalStateOperation operation)
    {
        return operation.RequestCase switch
        {
            TransactionalStateOperation.RequestOneofCase.Delete => new StateStoreTransactDeleteOperation(ToDeleteRequest(operation.Delete)),
            TransactionalStateOperation.RequestOneofCase.Set => new StateStoreTransactSetOperation(ToSetRequest(operation.Set)),
            _ => null
        };
    }

    public static StateStoreDeleteRequest ToDeleteRequest(DeleteRequest request)
    {
        return new StateStoreDeleteRequest(request.Key)
        {
            ETag = request.Etag?.Value,
            Metadata = request.Metadata
        };
    }

    public static StateStoreSetRequest ToSetRequest(SetRequest request)
    {
        return new StateStoreSetRequest(request.Key, request.Value.Memory)
        {
            ContentType = request.ContentType,
            ETag = request.Etag?.Value,
            Metadata = request.Metadata
        };
    }
}
