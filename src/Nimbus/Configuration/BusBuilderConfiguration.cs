using System;
using Nimbus.Exceptions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration
    {
        internal string ApplicationName { get; set; }
        internal string InstanceName { get; set; }
        internal string ConnectionString { get; set; }
        internal ICommandBroker CommandBroker { get; set; }
        internal IRequestBroker RequestBroker { get; set; }
        internal IMulticastEventBroker MulticastEventBroker { get; set; }
        internal ICompetingEventBroker CompetingEventBroker { get; set; }
        internal Type[] CommandHandlerTypes { get; set; }
        internal Type[] CommandTypes { get; set; }
        internal Type[] RequestHandlerTypes { get; set; }
        internal Type[] RequestTypes { get; set; }
        internal Type[] MulticastEventHandlerTypes { get; set; }
        internal Type[] CompetingEventHandlerTypes { get; set; }
        internal Type[] EventTypes { get; set; }
        internal TimeSpan DefaultTimeout { get; set; }
        internal int MaxDeliveryAttempts { get; set; }
        internal ILogger Logger { get; set; }
        internal BusDebuggingConfiguration Debugging { get; set; }

        internal BusBuilderConfiguration()
        {
            DefaultTimeout = TimeSpan.FromSeconds(10);
            MaxDeliveryAttempts = 5;
            Logger = new NullLogger();

            Debugging = new BusDebuggingConfiguration();
        }

        public Bus Build()
        {
            AssertConfigurationIsValid();
            return BusBuilder.Build(this);
        }

        private void AssertConfigurationIsValid()
        {
            //FIXME nowhere near done yet.  -andrewh 6/11/2013
            if (MulticastEventBroker == null) throw new BusConfigurationException("MulticastEventBroker", "You must supply a multicast event broker.");
            if (CompetingEventBroker == null) throw new BusConfigurationException("CompetingEventBroker", "You must supply a competing event broker.");
        }
    }
}