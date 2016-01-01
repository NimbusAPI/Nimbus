using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class TestForBus
    {
        protected const int TimeoutSeconds = 60;

        protected ScenarioInstance<BusBuilderConfiguration> Instance { get; private set; }
        protected Bus Bus { get; private set; }

        protected virtual async Task Given(IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            MethodCallCounter.Clear();

            Instance = scenario.CreateInstance();
            Reconfigure();

            Bus = Instance.Configuration.Build();
            await Bus.Start();
        }

        protected virtual void Reconfigure()
        {
        }

        protected abstract Task When();

        [SetUp]
        public void SetUp()
        {
            TestLoggingExtensions.LogTestStart();
        }

        [TearDown]
        public void TearDown()
        {
            var bus = Bus;
            Bus = null;

            bus?.Stop().Wait();
            bus?.Dispose();

            Instance?.Dispose();
            Instance = null;

            TestLoggingExtensions.LogTestResult();
        }
    }
}