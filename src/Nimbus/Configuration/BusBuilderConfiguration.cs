using System;
using Nimbus.Configuration.Settings;
using Nimbus.Exceptions;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration
    {
        internal ApplicationNameSetting ApplicationName { get; set; }
        internal InstanceNameSetting InstanceName { get; set; }
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal ICommandHandlerFactory CommandHandlerFactory { get; set; }
        internal IRequestBroker RequestBroker { get; set; }
        internal IMulticastRequestHandlerFactory MulticastRequestHandlerFactory { get; set; }
        internal IMulticastEventHandlerFactory MulticastEventHandlerFactory { get; set; }
        internal ICompetingEventHandlerFactory CompetingEventHandlerFactory { get; set; }

        internal CommandHandlerTypesSetting CommandHandlerTypes { get; set; }
        internal CommandTypesSetting CommandTypes { get; set; }
        internal RequestHandlerTypesSetting RequestHandlerTypes { get; set; }
        internal RequestTypesSetting RequestTypes { get; set; }
        internal MulticastEventHandlerTypesSetting MulticastEventHandlerTypes { get; set; }
        internal CompetingEventHandlerTypesSetting CompetingEventHandlerTypes { get; set; }
        internal EventTypesSetting EventTypes { get; set; }
        internal DefaultTimeoutSetting DefaultTimeout { get; set; }
        internal MaxDeliveryAttemptSetting MaxDeliveryAttempts { get; set; }
        internal ILogger Logger { get; set; }
        internal BusDebuggingConfiguration Debugging { get; set; }

        internal BusBuilderConfiguration()
        {
            DefaultTimeout = new DefaultTimeoutSetting {Value = TimeSpan.FromSeconds(10)}; //FIXME refactor these to override the Default property on their setting class
            MaxDeliveryAttempts = new MaxDeliveryAttemptSetting {Value = 5};
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
            if (MulticastEventHandlerFactory == null) throw new BusConfigurationException("MulticastEventBroker", "You must supply a multicast event broker.");
            if (CompetingEventHandlerFactory == null) throw new BusConfigurationException("CompetingEventBroker", "You must supply a competing event broker.");
            if (MulticastRequestHandlerFactory == null) throw new BusConfigurationException("MulticastRequestBroker", "You must supply a multicast request broker.");
        }
    }
}