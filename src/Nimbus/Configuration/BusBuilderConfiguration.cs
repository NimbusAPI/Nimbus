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
        internal ICommandBroker CommandBroker { get; set; }
        internal IRequestBroker RequestBroker { get; set; }
        internal IMulticastRequestBroker MulticastRequestBroker { get; set; }
        internal IMulticastEventBroker MulticastEventBroker { get; set; }
        internal ICompetingEventBroker CompetingEventBroker { get; set; }

        internal CommandHandlerTypesSetting CommandHandlerTypes { get; set; }
        internal CommandTypesSetting CommandTypes { get; set; }
        internal RequestHandlerTypesSetting RequestHandlerTypes { get; set; }
        internal RequestTypesSetting RequestTypes { get; set; }
        internal MulticastEventHandlerTypesSetting MulticastEventHandlerTypes { get; set; }
        internal CompetingEventHandlerTypesSetting CompetingEventHandlerTypes { get; set; }
        internal EventTypesSetting EventTypes { get; set; }
        internal DefaultTimeoutSetting DefaultTimeout { get; set; }
        internal BatchReceiveTimeoutSetting BatchReceiveTimeout { get; set; }
        internal MaxDeliveryAttemptSetting MaxDeliveryAttempts { get; set; }
        internal DefaultBatchSizeSetting DefaultBatchSize { get; set; }
        internal ILogger Logger { get; set; }
        internal BusDebuggingConfiguration Debugging { get; set; }

        internal BusBuilderConfiguration()
        {
            DefaultTimeout = new DefaultTimeoutSetting {Value = TimeSpan.FromSeconds(10)}; //FIXME refactor these to override the Default property on their setting class
            MaxDeliveryAttempts = new MaxDeliveryAttemptSetting {Value = 5};
            DefaultBatchSize = new DefaultBatchSizeSetting {Value = 16};
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
            if (MulticastRequestBroker == null) throw new BusConfigurationException("MulticastRequestBroker", "You must supply a multicast request broker.");
        }
    }
}