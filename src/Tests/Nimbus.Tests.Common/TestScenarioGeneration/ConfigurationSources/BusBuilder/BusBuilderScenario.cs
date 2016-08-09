using System;
using Nimbus.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Extensions;
using Nimbus.Routing;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.SynchronizationContexts;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.BusBuilder
{
    internal class BusBuilderScenario : CompositeScenario, IConfigurationScenario<BusBuilderConfiguration>
    {
        private readonly TestHarnessTypeProvider _typeProvider;
        private readonly ILogger _logger;
        private readonly IConfigurationScenario<TransportConfiguration> _transport;
        private readonly IConfigurationScenario<IRouter> _router;
        private readonly IConfigurationScenario<ISerializer> _serializer;
        private readonly IConfigurationScenario<ICompressor> _compressor;
        private readonly IConfigurationScenario<ContainerConfiguration> _iocContainer;
        private readonly IConfigurationScenario<SyncContextConfiguration> _syncContext;

        public BusBuilderScenario(TestHarnessTypeProvider typeProvider,
                                  ILogger logger,
                                  IConfigurationScenario<TransportConfiguration> transport,
                                  IConfigurationScenario<IRouter> router,
                                  IConfigurationScenario<ISerializer> serializer,
                                  IConfigurationScenario<ICompressor> compressor,
                                  IConfigurationScenario<ContainerConfiguration> iocContainer,
                                  IConfigurationScenario<SyncContextConfiguration> syncContext) : base(transport, router, serializer, compressor, iocContainer, syncContext)
        {
            _typeProvider = typeProvider;
            _logger = logger;
            _transport = transport;
            _router = router;
            _serializer = serializer;
            _compressor = compressor;
            _iocContainer = iocContainer;
            _syncContext = syncContext;
        }

        public ScenarioInstance<BusBuilderConfiguration> CreateInstance()
        {
            var globalPrefix = Guid.NewGuid().ToString();

            var transport = _transport.CreateInstance();
            var router = _router.CreateInstance();
            var serializer = _serializer.CreateInstance();
            var compressor = _compressor.CreateInstance();
            var iocContainer = _iocContainer.CreateInstance();
            var syncContext = _syncContext.CreateInstance();

            var configuration = new Nimbus.Configuration.BusBuilder()
                .Configure()
                .WithNames("MyTestSuite", Environment.MachineName)
                .WithGlobalPrefix(globalPrefix)
                .WithTransport(transport.Configuration)
                .WithRouter(router.Configuration)
                .WithSerializer(serializer.Configuration)
                .WithCompressor(compressor.Configuration)
                .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                .WithTypesFrom(_typeProvider)
                .WithHeartbeatInterval(TimeSpan.MaxValue)
                .WithLogger(_logger)
                .WithDebugOptions(
                    dc =>
                        dc.RemoveAllExistingNamespaceElementsOnStartup(
                            "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                .Chain(iocContainer.Configuration.ApplyContainerDefaults)
                ;

            var instance = new ScenarioInstance<BusBuilderConfiguration>(configuration);
            instance.Disposing += (s, e) => syncContext.Dispose();

            return instance;
        }
    }
}