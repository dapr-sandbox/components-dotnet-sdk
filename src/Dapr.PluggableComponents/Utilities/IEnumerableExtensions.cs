namespace Dapr.PluggableComponents.Utilities;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T?> items)
    {
        return items.Where(item => item != null).Cast<T>();
    }
}