using System;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Extensions.Pulse.Configuration
{
    public class PulseEnabledBusBuilderConfiguration
    {
        private readonly BusBuilderConfiguration _inner;
        private readonly PulseScheduleEntry[] _schedules;
        private IPulseCoordinator _coordinator = new NullPulseCoordinator();

        internal PulseEnabledBusBuilderConfiguration(BusBuilderConfiguration inner, PulseScheduleEntry[] schedules)
        {
            _inner = inner;
            _schedules = schedules;
        }

        /// <summary>
        ///     The application name the bus was configured with. Coordinator implementations should scope
        ///     their claims by this so that separate applications don't contend with each other, while
        ///     instances of the same application do.
        /// </summary>
        public string ApplicationName => _inner.ApplicationName;

        public ILogger Logger => _inner.Logger;

        /// <summary>
        ///     Elects a single instance to fire each occurrence. Defaults to <see cref="NullPulseCoordinator" />,
        ///     under which every instance fires everything.
        /// </summary>
        public PulseEnabledBusBuilderConfiguration WithCoordinator(IPulseCoordinator coordinator)
        {
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            return this;
        }

        public Bus Build()
        {
            var bus = _inner.Build();
            var coordinator = _coordinator;
            var engine = new PulseEngine(bus, _schedules, coordinator, _inner.Logger);

            bus.Started += async (_, _) => await engine.Start();
            bus.Stopping += async (_, _) => await engine.Stop();

            if (coordinator is IDisposable disposableCoordinator)
                bus.Disposing += (_, _) => disposableCoordinator.Dispose();

            return bus;
        }
    }
}
