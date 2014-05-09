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

        [SetUp]
        public void SetUp()
        {
            MethodCallCounter.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine();
            Console.WriteLine();
            Bus.Dispose();
        }

        public virtual async Task Given()
        {
            Bus = await new TestHarnessBusFactory(GetType()).CreateAndStart();

            Console.WriteLine();
            Console.WriteLine();
        }

        public abstract Task When();
    }
}