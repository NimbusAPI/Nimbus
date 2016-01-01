using System;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Transport;
using Nimbus.Extensions;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.Routing;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal class BusBuilderScenario : CompositeScenario, IConfigurationScenario<BusBuilderConfiguration>
    {
        private readonly TestHarnessTypeProvider _typeProvider;
        private readonly ILogger _logger;
        private readonly IConfigurationScenario<TransportConfiguration> _transport;
        private readonly IConfigurationScenario<IRouter> _router;
        private readonly IConfigurationScenario<ISerializer> _serializer;
        private readonly IConfigurationScenario<ContainerConfiguration> _iocContainer;

        public BusBuilderScenario(TestHarnessTypeProvider typeProvider,
                                  ILogger logger,
                                  IConfigurationScenario<TransportConfiguration> transport,
                                  IConfigurationScenario<IRouter> router,
                                  IConfigurationScenario<ISerializer> serializer,
                                  IConfigurationScenario<ContainerConfiguration> iocContainer) : base(transport, router, serializer, iocContainer)
        {
            _typeProvider = typeProvider;
            _logger = logger;
            _transport = transport;
            _router = router;
            _serializer = serializer;
            _iocContainer = iocContainer;
        }

        public ScenarioInstance<BusBuilderConfiguration> CreateInstance()
        {
            var transport = _transport.CreateInstance();
            var router = _router.CreateInstance();
            var serializer = _serializer.CreateInstance();
            var iocContainer = _iocContainer.CreateInstance();

            var configuration = new BusBuilder().Configure()
                                                .WithTransport(transport.Configuration)
                                                .WithRouter(router.Configuration)
                                                .WithSerializer(serializer.Configuration)
                                                .WithDeliveryRetryStrategy(new ImmediateRetryDeliveryStrategy())
                                                .WithNames("MyTestSuite", Environment.MachineName)
                                                .WithTypesFrom(_typeProvider)
                                                .WithGlobalInboundInterceptorTypes(_typeProvider.InterceptorTypes.Where(t => typeof (IInboundInterceptor).IsAssignableFrom(t)).ToArray())
                                                .WithGlobalOutboundInterceptorTypes(_typeProvider.InterceptorTypes.Where(t => typeof (IOutboundInterceptor).IsAssignableFrom(t)).ToArray())
                                                .WithHeartbeatInterval(TimeSpan.MaxValue)
                                                .WithLogger(_logger)
                                                .WithDebugOptions(
                                                    dc =>
                                                        dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                            "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                                .Chain(iocContainer.Configuration.ApplyContainerDefaults)
                ;

            return new ScenarioInstance<BusBuilderConfiguration>(configuration);
        }
    }
}