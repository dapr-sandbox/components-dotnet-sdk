using Google.Protobuf.Collections;

namespace Dapr.PluggableComponents.Utilities;

internal static class MapFieldExtensions
{
    public static void Add<TKey, TValue>(this MapField<TKey, TValue> map, IEnumerable<KeyValuePair<TKey, TValue>>? entries)
    {
        if (entries != null)
        {
            foreach (var entry in entries)
            {
                map.Add(entry.Key, entry.Value);
            }
        }
    }
}
