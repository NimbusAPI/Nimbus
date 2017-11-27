﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> Do<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }

        internal static void Done<T>(this IEnumerable<T> items)
        {
            // just force enumeration so that any chained .Do(...) calls are executed
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
            }
        }

        internal static bool None<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }

        internal static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return !items.Any(predicate);
        }

        internal static IEnumerable<T> DepthFirst<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> children)
        {
            foreach (var item in source)
            {
                yield return item;
                foreach (var descendant in children(item)) yield return descendant;
            }
        }

        internal static IEnumerable<T> NotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(item => item != null);
        }

        internal static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, Func<T, object> key)
        {
            return source.GroupBy(key).Select(g => g.First());
        }
    }
}