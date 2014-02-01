using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.InfrastructureContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(30*1000)]
    public abstract class TestForAllBuses
    {
        protected Bus Bus { get; private set; }

        [SetUp]
        public void SetUp()
        {
            MethodCallCounter.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Bus.Stop();
        }

        public virtual async Task Given(ITestHarnessBusFactory busFactory)
        {
            Bus = (Bus) busFactory.Create();
        }

        public abstract Task When();

        public IEnumerable<TestCaseData> AllBusesTestCases
        {
            get
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                var testFixtureType = GetType();
                var busFactoryEnumerator = new BusFactoryEnumerator(testFixtureType);
                foreach (var factory in busFactoryEnumerator.GetBusFactories())
                {
                    yield return new TestCaseData(factory)
                        .SetName(factory.MessageBrokerName)
                        ;
                }
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }
    }
}