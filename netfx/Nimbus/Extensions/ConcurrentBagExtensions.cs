﻿using System.Collections.Concurrent;

namespace Nimbus.Extensions
{
    internal static class ConcurrentBagExtensions
    {
        internal static void Clear<T>(this ConcurrentBag<T> bag)
        {
            T dummy;
            while (bag.TryTake(out dummy))
            {
            }
        }
    }
}