using Dapr.Proto.Components.V1;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record StateStoreQueryPagination
{
    public long Limit { get; init; }

    public string? Token { get; init; }

    internal static StateStoreQueryPagination? FromPagination(Pagination? pagination)
    {
        return pagination != null
            ? new StateStoreQueryPagination
            {
                Limit = pagination.Limit,
                Token = pagination.Token
            }
            : null;
    }
}
