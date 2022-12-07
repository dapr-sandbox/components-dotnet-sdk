using Google.Protobuf.Collections;

namespace Dapr.PluggableComponents.Proxies.Utilities;

internal static class MapFieldExtensions
{
    public static void Add<TKey, TValue>(this MapField<TKey, TValue> map, IEnumerable<KeyValuePair<TKey, TValue>> entries)
    {
        foreach (var entry in entries)
        {
            map.Add(entry.Key, entry.Value);
        }
    }
}
