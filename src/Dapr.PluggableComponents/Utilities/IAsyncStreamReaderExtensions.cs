using System.Runtime.CompilerServices;
using Grpc.Core;

namespace Dapr.PluggableComponents.Utilities;

internal static class IAsyncStreamReaderExtensions
{
    public static async IAsyncEnumerable<T> AsEnumerable<T>(this IAsyncStreamReader<T> reader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        while (await reader.MoveNext(cancellationToken))
        {
            yield return reader.Current;
        }
    }
}
