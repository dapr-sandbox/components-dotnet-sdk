using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using Dapr.Proto.Components.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Dapr.PluggableComponents.Services;

public class StateStoreWrapper : StateStore.StateStoreBase
{
    private readonly ILogger<StateStoreWrapper> _logger;
    private readonly IStateStore _backend;

    public StateStoreWrapper(ILogger<StateStoreWrapper> logger, IStateStore backend)
    {
        this._logger = logger;
        this._backend = backend;
    }

    public override Task<Empty> Init(MetadataRequest request, ServerCallContext context)
    {
        var props = new Dictionary<string, string>();
        foreach (var k in request.Properties.Keys)
        {
            props[k] = request.Properties[k];
        }
        _logger.LogInformation("Initializing state store backend");
        _backend.Init(props);
        return Task.FromResult(new Empty());
    }

    public override Task<FeaturesResponse> Features(Empty request, ServerCallContext context)
    {
        List<String> unused = _backend.Features();
        var resp = new FeaturesResponse();
        return Task.FromResult(resp);
    }

    public override async Task<Empty> Delete(DeleteRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Deleting data in store for key {}", request.Key);
        await _backend.Delete(request.Key, int.Parse(request.Etag.Value));
        return await base.Delete(request, context);
    }

    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Getting data in store for key {}", request.Key);

        var resp = new GetResponse();

        await _backend.Get(request.Key).ContinueWith(it =>
        {
            if (it.Result.HasValue)
            {
                var obj = it.Result;
                resp.Data = ByteString.CopyFrom(obj.Value.data);
                resp.Etag = new Etag { Value = obj.Value.etag.ToString() };
            }
            else
            {
                resp.Data = ByteString.Empty;
                resp.Etag = null;
            }
        });

        foreach (var k in request.Metadata.Keys)
        {
            resp.Metadata[k] = request.Metadata[k];
        }

        return resp;
    }

    public override async Task<Empty> Set(SetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Setting data in store for key {0}", request.Key);

        var obj = new StoreObject { data = request.Value.ToByteArray(), etag = -1 };

        await _backend.Set(request.Key, obj);
        return new Empty();
    }

    public override Task<Empty> Ping(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    public override async Task<Empty> BulkDelete(BulkDeleteRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk deleting data in store for {} keys", request.Items.Count);

        await Task.WhenAll(request.Items.Select(item => _backend.Delete(item.Key, int.Parse(item.Etag.Value))));
        return new Empty();
    }

    public override async Task<BulkGetResponse> BulkGet(BulkGetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk fetching data in store for {} keys", request.Items.Count);

        var response = new BulkGetResponse();
        var responsesTasks = request.Items.Select(async item =>
        {
            var storeObj = await _backend.Get(item.Key);
            return storeObj != null ? new BulkStateItem
            {
                Data = ByteString.CopyFrom(storeObj.Value.data),
                Etag = new Etag { Value = storeObj.Value.etag.ToString() },
                Key = item.Key,
                Error = "none"
            } : new BulkStateItem
            {
                Data = ByteString.Empty,
                Etag = new Etag(),
                Key = item.Key,
                Error = "KeyDoesNotExist"
            };
        });

        var itemsResponse = await Task.WhenAll(responsesTasks);
        response.Items.AddRange(itemsResponse);

        return response;
    }

    public override async Task<Empty> BulkSet(BulkSetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk storing data in store for {} keys", request.Items.Count);

        var setRequests = request.Items.Select(async item =>
        {
            await Set(item, context);
        });
        await Task.WhenAll(setRequests);

        return new Empty();
    }
}
