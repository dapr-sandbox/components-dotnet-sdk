namespace Dapr.PluggableComponents.Components.StateStore;

public sealed record QueryRequestPagination
{
    public long Limit { get; init; }

    public string? Token { get; init; }
}
