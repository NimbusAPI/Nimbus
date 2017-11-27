using System;
using Nimbus.Extensions;

namespace Nimbus.Configuration.LargeMessages
{
    public static class DefaultStorageKeyGenerator
    {
        public static string GenerateStorageKey(Guid id, DateTimeOffset expiresAfter)
        {
            return "{0}/{1}/{2}/{3}/{4}/{5}".FormatWith(expiresAfter.Year, expiresAfter.Month, expiresAfter.Day, expiresAfter.Hour, expiresAfter.Minute, id);
        }
    }
}