using System.Reflection;
using Cronos;

namespace Nimbus.Extensions.Pulse
{
    internal record PulseScheduleEntry(string Name, CronExpression Cron, object Message, bool IsCommand)
    {
        private static readonly MethodInfo _memberwiseClone =
            typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     Produces a fresh copy of the configured message for each fire. The configured instance is
        ///     a template: handing it straight to the bus means stamping PulseTime onto an object that a
        ///     previous send may still be serialising, and in-process handlers would see it mutate
        ///     underneath them.
        ///     This is a shallow copy, so reference-typed properties are still shared with the template.
        ///     Pulse messages are expected to be flat DTOs; if yours isn't, don't mutate it in a handler.
        /// </summary>
        internal object CreateMessage()
        {
            return _memberwiseClone.Invoke(Message, null);
        }
    }
}
