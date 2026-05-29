using System;
using System.Linq;
using Cronos;
using Nimbus.Configuration;
using Nimbus.MessageContracts;

namespace Nimbus.Extensions.Pulse.Configuration
{
    public static class BusBuilderConfigurationPulseExtensions
    {
        public static PulseEnabledBusBuilderConfiguration WithPulse(
            this BusBuilderConfiguration config,
            params (string cronExpression, object message)[] schedules)
        {
            var entries = schedules.Select(s =>
            {
                if (s.message is not IBusCommand && s.message is not IBusEvent)
                    throw new ArgumentException(
                        $"Pulse message '{s.message.GetType().Name}' must implement either IBusCommand or IBusEvent.",
                        nameof(schedules));

                return new PulseScheduleEntry(
                    CronExpression.Parse(s.cronExpression),
                    s.message,
                    s.message is IBusCommand);
            }).ToArray();

            return new PulseEnabledBusBuilderConfiguration(config, entries);
        }
    }
}
