using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreBulkGetResponse(bool Got)
{
    public IReadOnlyList<StateStoreBulkStateItem> Items { get; init; } = new List<StateStoreBulkStateItem>();

    internal static BulkGetResponse ToBulkGetResponse(StateStoreBulkGetResponse response)
    {
        var bulkGetResponse = new BulkGetResponse
        {
            Got = response.Got
        };

        bulkGetResponse.Items.AddRange(
            response
                .Items
                .Select(StateStoreBulkStateItem.ToBulkStateItem)
                .ToList());
        
        return bulkGetResponse;
    }
}