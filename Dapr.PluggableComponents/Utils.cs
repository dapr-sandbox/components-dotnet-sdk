using Dapr.Client.Autogen.Grpc.v1;
using pbc = Google.Protobuf.Collections;

namespace Dapr.PluggableComponents;

public static class Utils
{
    public static Dictionary<string, string> ConvertMetadata(pbc.MapField<string, string> metadata)
    {
        var target = new Dictionary<string, string>();
        foreach (var k in metadata)
        {
            target[k.Key] = k.Value;
        }

        return target;
    }

    public static void MergeDictionaryIntoMetadata(Dictionary<string, string> dict,
        pbc.MapField<string, string> metadata)
    {
        foreach (var k in dict.Keys)
        {
            metadata[k] = dict[k];
        }
    }

    public static MetadataRequest MetadataFromDictionary(Dictionary<string, string> data)
    {
        var req = new MetadataRequest();
        foreach (var k in data.Keys)
        {
            req.Properties.Add(k, data[k]);
        }

        return req;
    }
}


