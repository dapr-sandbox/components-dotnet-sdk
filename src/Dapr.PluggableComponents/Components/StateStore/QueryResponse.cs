namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class QueryResponseQueryItem
{
    public string Key { get; init; }
    public byte[] Data { get; init; }
    public string ETag { get; init; }
    public string Error { get; init; }
    public string ContentType { get; init; }
}

public sealed class QueryResponse
{
    public QueryResponseQueryItem[] Items { get; init; } = Array.Empty<QueryResponseQueryItem>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    public string Token { get; init; }
}