using System;

namespace Nimbus.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        internal static DateTimeOffset AddSafely(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.MaxValue) return DateTimeOffset.MaxValue;
            if (timeSpan == TimeSpan.MinValue) return DateTimeOffset.MinValue;

            if (timeSpan >= TimeSpan.Zero)
            {
                if (dateTimeOffset > DateTimeOffset.MaxValue - timeSpan) return DateTimeOffset.MaxValue;
                if (dateTimeOffset < DateTimeOffset.MinValue + timeSpan) return DateTimeOffset.MinValue;
            }
            else
            {
                if (dateTimeOffset > DateTimeOffset.MaxValue + timeSpan) return DateTimeOffset.MaxValue;
                if (dateTimeOffset < DateTimeOffset.MinValue - timeSpan) return DateTimeOffset.MinValue;
            }

            return dateTimeOffset.Add(timeSpan);
        }
    }
}