using System;
using System.Linq;

namespace Nimbus.Extensions.Pulse
{
    internal static class PulseScheduleName
    {
        /// <summary>
        ///     Builds the identity a coordinator claims against. It has to be stable across restarts and
        ///     identical on every instance, so it's derived only from the message type and the cron
        ///     expression — never from anything instance-local.
        /// </summary>
        internal static string For(Type messageType, string cronExpression)
        {
            return $"{messageType.FullName}:{NormaliseCron(cronExpression)}";
        }

        /// <summary>
        ///     Collapses runs of whitespace to a single underscore so that "0 0 * * *" and "0  0 * * *"
        ///     produce the same name. Everything else in a cron expression is left alone — it's all
        ///     legal in a Redis key and it keeps the name readable in logs.
        /// </summary>
        private static string NormaliseCron(string cronExpression)
        {
            return string.Join("_", PulseCronExpression.Fields(cronExpression));
        }
    }
}
