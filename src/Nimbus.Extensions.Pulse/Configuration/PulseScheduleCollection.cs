using System;
using System.Collections.Generic;
using System.Linq;
using Cronos;
using Nimbus.MessageContracts;

namespace Nimbus.Extensions.Pulse.Configuration
{
    public class PulseScheduleCollection
    {
        private readonly List<Candidate> _candidates = new List<Candidate>();

        internal PulseScheduleCollection()
        {
        }

        /// <summary>
        ///     Adds a schedule.
        /// </summary>
        /// <param name="name">
        ///     Optional. Identifies the schedule to the coordinator, so it must be the same on every
        ///     instance and stable across restarts. Leave it null to have one derived from the message
        ///     type and cron expression.
        ///     Name a schedule explicitly when you run the same message type on two schedules that would
        ///     otherwise derive the same name, or when you want to change a cron expression without the
        ///     schedule's identity changing underneath any claims already in flight.
        /// </param>
        public PulseScheduleCollection Add(string cronExpression, object message, string name = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (message is not IBusCommand && message is not IBusEvent)
                throw new ArgumentException(
                    $"Pulse message '{message.GetType().Name}' must implement either IBusCommand or IBusEvent.",
                    nameof(message));

            if (string.IsNullOrWhiteSpace(cronExpression))
                throw new ArgumentException("A cron expression is required.", nameof(cronExpression));

            var isExplicitlyNamed = name != null;
            name = isExplicitlyNamed ? ValidateExplicitName(name) : PulseScheduleName.For(message.GetType(), cronExpression);

            _candidates.Add(new Candidate(name,
                                          PulseCronExpression.Parse(cronExpression),
                                          message,
                                          message is IBusCommand,
                                          isExplicitlyNamed));
            return this;
        }

        private static string ValidateExplicitName(string name)
        {
            var trimmed = name.Trim();

            if (trimmed.Length == 0)
                throw new ArgumentException(
                    "A Pulse schedule name cannot be blank. Pass null to have one derived from the message type and cron expression.",
                    nameof(name));

            if (trimmed.Any(char.IsControl))
                throw new ArgumentException(
                    $"Pulse schedule name '{name}' contains control characters. Names end up in coordinator keys and must be printable.",
                    nameof(name));

            return trimmed;
        }

        internal PulseScheduleEntry[] Build()
        {
            GuardAgainstNameClashes();
            return _candidates.Select(c => new PulseScheduleEntry(c.Name, c.Cron, c.Message, c.IsCommand)).ToArray();
        }

        /// <summary>
        ///     Two schedules sharing a name would contend for the same claim and only one of them would
        ///     ever fire, so this is a configuration error rather than something to resolve at runtime.
        ///     The comparison ignores case: Redis keys are case-sensitive, but a coordinator backed by a
        ///     SQL Server database with a case-insensitive collation would treat the two as one.
        /// </summary>
        private void GuardAgainstNameClashes()
        {
            var clash = _candidates.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                                   .FirstOrDefault(g => g.Count() > 1);
            if (clash == null) return;

            var explicitlyNamed = clash.Count(c => c.IsExplicitlyNamed);

            string advice;
            if (explicitlyNamed == 0)
                advice = "They share a message type and cron expression, so both derive the same name. Give at least one of them an explicit name.";
            else if (explicitlyNamed == clash.Count())
                advice = "Explicit schedule names must be unique.";
            else
                advice = "An explicit name collided with the name derived from another schedule's message type and cron expression. Rename one of them.";

            throw new ArgumentException(
                $"Pulse schedule name '{clash.Key}' is used by {clash.Count()} schedules. {advice} " +
                "Names identify occurrences to the coordinator, so duplicates would contend for the same claim and only one would fire.");
        }

        private record Candidate(string Name, CronExpression Cron, object Message, bool IsCommand, bool IsExplicitlyNamed);
    }
}
