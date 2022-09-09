using Dapr.Client.Autogen.Grpc.v1;
using Dapr.PluggableComponents.Components;
using proto = Dapr.Proto.Components.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Dapr.PluggableComponents.Services;

public class StateStoreWrapper : proto.StateStore.StateStoreBase
{
    private readonly ILogger<StateStoreWrapper> _logger;
    private readonly IStateStore _backend;

    public StateStoreWrapper(ILogger<StateStoreWrapper> logger, IStateStore backend)
    {
        this._logger = logger;
        this._backend = backend;
    }

    public override Task<proto.InitResponse> Init(proto.InitRequest request, ServerCallContext context)
    {
        var props = new Dictionary<string, string>();
        foreach (var k in request.Metadata.Properties.Keys)
        {
            props[k] = request.Metadata.Properties[k];
        }
        _logger.LogInformation("Initializing state store backend");
        _backend.Init(props);
        return Task.FromResult(new proto.InitResponse());
    }

    public override Task<FeaturesResponse> Features(FeaturesRequest _, ServerCallContext context)
    {
        List<String> unused = _backend.Features();
        var resp = new FeaturesResponse();
        return Task.FromResult(resp);
    }

    public override async Task<proto.DeleteResponse> Delete(proto.DeleteRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Deleting data in store for key {}", request.Key);
        await _backend.Delete(request.Key, int.Parse(request.Etag.Value));
        return new proto.DeleteResponse();
    }

    public override async Task<proto.GetResponse> Get(proto.GetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Getting data in store for key {}", request.Key);

        var resp = new proto.GetResponse();

        await _backend.Get(request.Key).ContinueWith(it =>
        {
            if (it.Result.HasValue)
            {
                var obj = it.Result;
                resp.Data = ByteString.CopyFrom(obj.Value.Data);
                resp.Etag = new Etag { Value = obj.Value.ETag.ToString() };
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

    public override async Task<proto.SetResponse> Set(proto.SetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Setting data in store for key {0}", request.Key);

        var obj = new StoreObject { Data = request.Value.ToByteArray(), ETag = -1 };

        await _backend.Set(request.Key, obj);
        return new proto.SetResponse();
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse());
    }

    public override async Task<proto.BulkDeleteResponse> BulkDelete(proto.BulkDeleteRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk deleting data in store for {} keys", request.Items.Count);

        await Task.WhenAll(request.Items.Select(item => _backend.Delete(item.Key, int.Parse(item.Etag.Value))));
        return new proto.BulkDeleteResponse();
    }

    public override async Task<proto.BulkGetResponse> BulkGet(proto.BulkGetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk fetching data in store for {} keys", request.Items.Count);

        var response = new proto.BulkGetResponse();
        var responsesTasks = request.Items.Select(async item =>
        {
            var storeObj = await _backend.Get(item.Key);
            return storeObj != null ? new proto.BulkStateItem
            {
                Data = ByteString.CopyFrom(storeObj.Value.Data),
                Etag = new Etag { Value = storeObj.Value.ETag.ToString() },
                Key = item.Key,
                Error = "none"
            } : new proto.BulkStateItem
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

    public override async Task<proto.BulkSetResponse> BulkSet(proto.BulkSetRequest request, ServerCallContext context)
    {
        _logger.LogDebug("Bulk storing data in store for {} keys", request.Items.Count);

        var setRequests = request.Items.Select(async item =>
        {
            await Set(item, context);
        });
        await Task.WhenAll(setRequests);

        return new proto.BulkSetResponse();
    }
}
