using System;

namespace Nimbus.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        internal static DateTimeOffset AddSafely(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
        {
            try
            {
                return dateTimeOffset.Add(timeSpan);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTimeOffset.MaxValue;
            }
        }
    }
}