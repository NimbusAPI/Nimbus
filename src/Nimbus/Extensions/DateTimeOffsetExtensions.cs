using System;

namespace Nimbus.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        internal static DateTimeOffset AddSafely(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
        {
            var ticks = dateTimeOffset.Ticks + timeSpan.Ticks;
            if (ticks > DateTimeOffset.MaxValue.Ticks) return DateTimeOffset.MaxValue;
            if (ticks < DateTimeOffset.MinValue.Ticks) return DateTimeOffset.MinValue;
            return dateTimeOffset.Add(timeSpan);
        }
    }
}