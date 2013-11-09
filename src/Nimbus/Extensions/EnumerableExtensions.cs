using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Do<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }

        public static void Done<T>(this IEnumerable<T> items)
        {
            // just force enumeration so that any chained .Do(...) calls are executed

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            items.ToArray();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        public static bool None<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }

        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return !items.Any(predicate);
        }
    }
}