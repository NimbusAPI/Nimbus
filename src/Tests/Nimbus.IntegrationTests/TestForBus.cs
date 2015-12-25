using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class TestForBus
    {
        protected const int TimeoutSeconds = 15;

        protected Bus Bus { get; private set; }

        protected virtual async Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            MethodCallCounter.Clear();

            Bus = busBuilderConfiguration.Build();
            await Bus.Start();
        }

        protected abstract Task When();

        [TearDown]
        public void TearDown()
        {
            var bus = Bus;
            Bus = null;

            bus?.Dispose();
        }
    }
}