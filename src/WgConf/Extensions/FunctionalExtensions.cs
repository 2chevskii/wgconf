namespace WgConf.Extensions;

public static class FunctionalExtensions
{
    public static void Let<T>(this T? self, Action<T> action)
        where T : class
    {
        if (self == null)
        {
            return;
        }

        action(self);
    }

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
