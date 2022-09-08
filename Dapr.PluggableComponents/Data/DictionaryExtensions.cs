namespace Dapr.PluggableComponents.Data;
using pbc = Google.Protobuf.Collections;

public static class Extensions
{
    public static void CopyToMapField<T, U>(this Dictionary<T, U> input, pbc.MapField<T, U> target)
    {
        foreach (var k in input.Keys) {
            target[k] = input[k];
        }
    }
    
    public static Dictionary<T, U> ToDictionary<T, U>(this pbc.MapField<T, U> metadata)
    {
        var target = new Dictionary<T, U>();
        foreach (var k in metadata)
        {
            target[k.Key] = k.Value;
        }

        return target;
    }
}
