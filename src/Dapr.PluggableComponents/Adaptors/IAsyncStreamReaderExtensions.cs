using System.Runtime.CompilerServices;
using Grpc.Core;

namespace Dapr.PluggableComponents.Adaptors;

public static class IAsyncStreamReaderExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncStreamReader<T> reader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await reader.MoveNext(cancellationToken))
        {
            yield return reader.Current;
        }
    }
}