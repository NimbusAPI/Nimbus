using System;
using Cronos;

namespace Nimbus.Extensions.Pulse
{
    internal static class PulseCronExpression
    {
        /// <summary>
        ///     Parses either a standard five-field expression or a six-field one whose leading field is
        ///     seconds, picking the format from the field count. Cronos needs to be told which it's
        ///     looking at and rejects the other outright, so detecting it here means callers don't have to
        ///     care.
        /// </summary>
        internal static CronExpression Parse(string cronExpression)
        {
            var format = Fields(cronExpression).Length >= 6 ? CronFormat.IncludeSeconds : CronFormat.Standard;
            return CronExpression.Parse(cronExpression, format);
        }

        internal static string[] Fields(string cronExpression)
        {
            return cronExpression.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
