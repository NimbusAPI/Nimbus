using System;
using System.Linq;
using Nimbus.Configuration.Debug;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Routing;
using Nimbus.Logger;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Routing;

namespace Nimbus.Configuration
{
    public class BusBuilderConfiguration : INimbusConfiguration
    {
        internal ITypeProvider TypeProvider { get; set; }
        internal IDependencyResolver DependencyResolver { get; set; }
        internal ILogger Logger { get; set; }
        internal ISerializer Serializer { get; set; }
        internal ICompressor Compressor { get; set; }
        internal IRouter Router { get; set; }

        internal BusBuilderDebuggingConfiguration Debugging { get; set; }
        internal LargeMessageStorageConfiguration LargeMessageStorageConfiguration { get; set; }

        internal ApplicationNameSetting ApplicationName { get; set; }
        internal InstanceNameSetting InstanceName { get; set; }
        internal ConnectionStringSetting ConnectionString { get; set; }
        internal ServerConnectionCountSetting ServerConnectionCount { get; set; }
        internal DefaultTimeoutSetting DefaultTimeout { get; set; }
        internal DefaultMessageLockDurationSetting DefaultMessageLockDuration { get; set; }
        internal ConcurrentHandlerLimitSetting DefaultConcurrentHandlerLimit { get; set; }
        internal MaxDeliveryAttemptSetting MaxDeliveryAttempts { get; set; }
        internal GlobalInboundInterceptorTypesSetting GlobalInboundInterceptorTypes { get; set; }
        internal GlobalOutboundInterceptorTypesSetting GlobalOutboundInterceptorTypes { get; set; }

        internal BusBuilderConfiguration()
        {
            Debugging = new BusBuilderDebuggingConfiguration();
            LargeMessageStorageConfiguration = new LargeMessageStorageConfiguration();
            Router = new DestinationPerMessageTypeRouter();
            
            Logger = new NullLogger();
            Serializer = new DataContractSerializer();
            Compressor = new NullCompressor();
        }

        public Bus Build()
        {
            AssertConfigurationIsValid();
            return BusBuilder.Build(this);
        }

        private void AssertConfigurationIsValid()
        {
            var validatableComponents = GetType().GetProperties()
                                                 .Select(p => p.GetValue(this))
                                                 .OfType<IValidatableConfigurationSetting>()
                                                 .ToArray();

            var validationErrors = validatableComponents
                .SelectMany(c => c.Validate())
                .ToArray();

            if (validationErrors.None()) return;

            var message = string.Join(Environment.NewLine, new[] {"Bus configuration is invalid:"}.Concat(validationErrors));
            throw new BusException(message);
        }
    }
}