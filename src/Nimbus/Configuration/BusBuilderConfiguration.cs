using System;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration
    {
        private readonly BusBuilder _busBuilder;

        internal string InstanceName { get; set; }
        internal string ConnectionString { get; set; }
        internal ICommandBroker CommandBroker { get; set; }
        internal ITimeoutBroker TimeoutBroker { get; set; }
        internal IRequestBroker RequestBroker { get; set; }
        internal IEventBroker EventBroker { get; set; }
        internal Type[] EventHandlerTypes { get; set; }
        internal Type[] CommandHandlerTypes { get; set; }
        internal Type[] TimeoutHandlerTypes { get; set; }
        internal Type[] RequestHandlerTypes { get; set; }
        internal TimeSpan DefaultTimeout { get; set; }
        internal int MaxDeliveryAttempts { get; set; }
        internal ILogger Logger { get; set; }

        internal BusBuilderConfiguration(BusBuilder busBuilder)
        {
            _busBuilder = busBuilder;

            DefaultTimeout = TimeSpan.FromSeconds(10);
            MaxDeliveryAttempts = 5;
            Logger = new NullLogger();
        }

        public Bus Build()
        {
            return _busBuilder.Build();
        }
    }
}