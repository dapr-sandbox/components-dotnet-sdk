using Dapr.Proto.Components.V1;
using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreQueryPaginationTests
{
    [Fact]
    public void FromPaginationConversionTests()
    {
        var converter = StateStoreQueryPagination.FromPagination;
        StateStoreQueryPagination? curriedConverter<T>(T _, Pagination? pagination) => converter(pagination);

        ConversionAssert.Equal(
            limit => limit.HasValue ? new Pagination { Limit = limit.Value } : null,
            curriedConverter,
            pagination => pagination?.Limit,
            new (long?, long?)[]
            {
                (null, null),
                (0, 0),
                (123, 123)
            });

        ConversionAssert.Equal(
            token => token != null ? new Pagination { Token = token } : null,
            curriedConverter,
            pagination => pagination?.Token,
            new[]
            {
                (null, null),
                ("", ""),
                ("token", "token")
            });
    }
}
