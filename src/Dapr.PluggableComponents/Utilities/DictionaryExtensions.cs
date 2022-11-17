using Google.Protobuf.Collections;

namespace Dapr.PluggableComponents.Utilities;

public static class DictionaryExtensions
{
    public static void CopyTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, MapField<TKey, TValue> mapField)
    {
        foreach (var kvp in dictionary)
        {
            mapField.Add(kvp.Key, kvp.Value);
        }
    }

    public static MapField<TKey, TValue> ToMapField<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        var mapField = new MapField<TKey, TValue>();

        dictionary.CopyTo(mapField);

        return mapField;
    }
}