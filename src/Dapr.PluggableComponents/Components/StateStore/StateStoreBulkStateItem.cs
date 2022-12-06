using Dapr.PluggableComponents.Utilities;
using Dapr.Proto.Components.V1;
using Google.Protobuf;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkStateItem(string Key)
{
    public string? ContentType { get; init; }

    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string? Error { get; init; }

    public string? ETag { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    internal static BulkStateItem ToBulkStateItem(StateStoreBulkStateItem item)
    {
        var bulkStateItem = new BulkStateItem
        {
            ContentType = item.ContentType ?? String.Empty,
            Data = ByteString.CopyFrom(item.Data),
            Error = item.Error ?? String.Empty,
            Etag = item.ETag != null ? new Etag { Value = item.ETag } : null
        };

        bulkStateItem.Metadata.Add(item.Metadata);

        return bulkStateItem;
    }
}
