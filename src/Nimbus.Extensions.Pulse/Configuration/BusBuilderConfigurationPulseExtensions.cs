using System;
using Nimbus.Configuration;

namespace Nimbus.Extensions.Pulse.Configuration
{
    public static class BusBuilderConfigurationPulseExtensions
    {
        /// <summary>
        ///     Configures cron schedules, optionally naming them.
        /// </summary>
        /// <example>
        ///     <code>
        ///     .WithPulse(p => p.Add("0 3 * * *", new NightlyRollupCommand())
        ///                      .Add("0 4 * * *", new NightlyRollupCommand(), name: "rollup-retry")
        ///                      .Add("*/30 * * * * *", new HeartbeatCommand()))
        ///     </code>
        ///     Five-field expressions are minute-resolution; a six-field expression adds a leading
        ///     seconds field.
        /// </example>
        public static PulseEnabledBusBuilderConfiguration WithPulse(
            this BusBuilderConfiguration config,
            Action<PulseScheduleCollection> configureSchedules)
        {
            if (configureSchedules == null) throw new ArgumentNullException(nameof(configureSchedules));

            var schedules = new PulseScheduleCollection();
            configureSchedules(schedules);

            return new PulseEnabledBusBuilderConfiguration(config, schedules.Build());
        }

        /// <summary>
        ///     Configures cron schedules whose names are derived from the message type and cron
        ///     expression. Use the <see cref="PulseScheduleCollection" /> overload if you need to name one.
        /// </summary>
        public static PulseEnabledBusBuilderConfiguration WithPulse(
            this BusBuilderConfiguration config,
            params (string cronExpression, object message)[] schedules)
        {
            return config.WithPulse(p =>
                                    {
                                        foreach (var schedule in schedules)
                                            p.Add(schedule.cronExpression, schedule.message);
                                    });
        }
    }
}
