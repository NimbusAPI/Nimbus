using Nimbus.Configuration.Debug;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Logger;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration
    {
        internal ITypeProvider TypeProvider { get; set; }
        internal IDependencyResolver DependencyResolver { get; set; }
        internal ILogger Logger { get; set; }
        internal ISerializer Serializer { get; set; }
        internal ICompressor Compressor { get; set; }

        internal BusBuilderDebuggingConfiguration Debugging { get; set; }
        internal ILargeMessageBodyConfiguration LargeMessageBodyConfiguration { get; set; }

        internal ApplicationNameSetting ApplicationName { get; set; }
        internal InstanceNameSetting InstanceName { get; set; }
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal CommandHandlerTypesSetting CommandHandlerTypes { get; set; }
        internal CommandTypesSetting CommandTypes { get; set; }
        internal RequestHandlerTypesSetting RequestHandlerTypes { get; set; }
        internal RequestTypesSetting RequestTypes { get; set; }
        internal MulticastEventHandlerTypesSetting MulticastEventHandlerTypes { get; set; }
        internal CompetingEventHandlerTypesSetting CompetingEventHandlerTypes { get; set; }
        internal EventTypesSetting EventTypes { get; set; }

        internal ServerConnectionCountSetting ServerConnectionCount { get; set; }
        internal DefaultTimeoutSetting DefaultTimeout { get; set; }
        internal DefaultMessageLockDurationSetting DefaultMessageLockDuration { get; set; }
        internal ConcurrentHandlerLimitSetting DefaultConcurrentHandlerLimit { get; set; }
        internal MaxDeliveryAttemptSetting MaxDeliveryAttempts { get; set; }

        internal BusBuilderConfiguration()
        {
            Debugging = new BusBuilderDebuggingConfiguration();

            Logger = new NullLogger();
            Serializer = new DataContractSerializer();
            Compressor = new NullCompressor();
            LargeMessageBodyConfiguration = new UnsupportedLargeMessageBodyConfiguration();
        }

        public Bus Build()
        {
            AssertConfigurationIsValid();
            return BusBuilder.Build(this);
        }

        private void AssertConfigurationIsValid()
        {
            //FIXME nowhere near done yet.  -andrewh 6/11/2013
        }
    }
}