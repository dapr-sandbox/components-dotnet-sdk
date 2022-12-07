using System.Runtime.CompilerServices;

namespace Dapr.PluggableComponents.Utilities;

internal static class IAsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TTransformedType> WithTransform<TOriginalType, TTransformedType>(this IAsyncEnumerable<TOriginalType> enumerable, Func<TOriginalType, TTransformedType> transform, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        var enumerator = enumerable.GetAsyncEnumerator(cancellationToken);

        try
        {
            while (await enumerator.MoveNextAsync())
            {
                yield return transform(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }
}
