using Google.Protobuf.Collections;

namespace Dapr.PluggableComponents.Utilities;

public static class MapFieldExtensions
{
    public static void Add<TKey, TValue>(this MapField<TKey, TValue> map, IEnumerable<KeyValuePair<TKey, TValue>> entries)
    {
        foreach (var entry in entries)
        {
            map.Add(entry.Key, entry.Value);
        }
    }
}
