using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public abstract class TestForBus
    {
        protected Bus Bus { get; private set; }

        protected virtual async Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            MethodCallCounter.Clear();

            Bus = busBuilderConfiguration.Build();
            await busBuilderConfiguration.Build().Start();
        }

        protected abstract Task When();

        [TearDown]
        public void TearDown()
        {
            Bus.Dispose();
        }
    }
}