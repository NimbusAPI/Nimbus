using Nimbus.Configuration;

namespace Nimbus.Extensions.Pulse.Configuration
{
    public class PulseEnabledBusBuilderConfiguration
    {
        private readonly BusBuilderConfiguration _inner;
        private readonly PulseScheduleEntry[] _schedules;

        internal PulseEnabledBusBuilderConfiguration(BusBuilderConfiguration inner, PulseScheduleEntry[] schedules)
        {
            _inner = inner;
            _schedules = schedules;
        }

        public Bus Build()
        {
            var bus = _inner.Build();
            var engine = new PulseEngine(bus, _schedules);
            bus.Started += async (_, _) => await engine.Start();
            bus.Stopping += async (_, _) => await engine.Stop();
            return bus;
        }
    }
}
