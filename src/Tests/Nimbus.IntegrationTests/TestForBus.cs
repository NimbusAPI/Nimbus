using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(15*1000)]
    public abstract class TestForBus
    {
        protected Bus Bus { get; private set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Task.Run(async () =>
            {
                MethodCallCounter.Clear();

                Bus = await new TestHarnessBusFactory(GetType()).CreateAndStart();
                Console.WriteLine();
                Console.WriteLine();

                await Given();
                Console.WriteLine();
                Console.WriteLine();

                await When();
                Console.WriteLine();
                Console.WriteLine();
            }).Wait();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Console.WriteLine();
            Console.WriteLine();
            Bus.Dispose();
        }

        protected virtual async Task Given()
        {
        }

        protected abstract Task When();
    }
}