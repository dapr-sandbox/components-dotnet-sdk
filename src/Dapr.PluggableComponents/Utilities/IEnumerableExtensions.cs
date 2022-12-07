namespace Dapr.PluggableComponents.Utilities;

internal static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T?> items)
    {
        return items.Where(item => item != null).Cast<T>();
    }
}