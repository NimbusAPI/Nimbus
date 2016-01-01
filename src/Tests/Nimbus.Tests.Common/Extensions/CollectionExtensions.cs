using System.Collections.Generic;
using Nimbus.Extensions;

namespace Nimbus.Tests.Common.Extensions
{
    internal static class CollectionExtensions
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> items)
        {
            items
                .Do(item => hashSet.Add(item))
                .Done();
        }   
    }
}