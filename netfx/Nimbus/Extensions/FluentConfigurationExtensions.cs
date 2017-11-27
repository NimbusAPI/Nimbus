using System;

namespace Nimbus.Extensions
{
    internal static class FluentConfigurationExtensions
    {
        internal static T Chain<T>(this T t, Func<T, T> apply)
        {
            return apply(t);
        }

        internal static T Chain<T>(this T t, Action<T> apply)
        {
            apply(t);
            return t;
        }
    }
}