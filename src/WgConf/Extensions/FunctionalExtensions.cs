namespace WgConf.Extensions;

internal static class FunctionalExtensions
{
    /// <summary>
    /// Executes an action when the reference type value is not null.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="self">The value to test.</param>
    /// <param name="action">The action to invoke when a value is present.</param>
    public static void Let<T>(this T? self, Action<T> action)
        where T : class
    {
        if (self == null)
        {
            return;
        }

        action(self);
    }

    /// <summary>
    /// Executes an action when the nullable value type has a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="self">The nullable value to test.</param>
    /// <param name="action">The action to invoke when a value is present.</param>
    public static void Let<T>(this T? self, Action<T> action)
        where T : struct
    {
        if (!self.HasValue)
        {
            return;
        }

        action(self.Value);
    }
}
