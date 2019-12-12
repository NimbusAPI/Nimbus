using System;
using System.Collections.Generic;
using Nimbus.Extensions;

namespace Nimbus.Tests.Integration.Extensions
{
    internal static class CollectionExtensions
    {
        internal static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            items
                .Do(item => hashSet.Add(item))
                .Done();
        }

        internal static IEnumerable<TOutput> Pipe<TInput, TOutput>(this IEnumerable<TInput> source, Func<IEnumerable<TInput>, IEnumerable<TOutput>> filter)
        {
            return filter(source);
        }
    }
}