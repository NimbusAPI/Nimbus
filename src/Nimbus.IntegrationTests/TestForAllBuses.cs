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
        [SetUp]
        public void SetUp()
        {
            MethodCallCounter.Clear();
        }

        [TearDown]
        public void TearDown()
        {
        }

        public abstract Task When(ITestHarnessBusFactory busFactory);

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
                        .SetName(factory.BusFactoryName)
                        ;
                }
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }
    }
}