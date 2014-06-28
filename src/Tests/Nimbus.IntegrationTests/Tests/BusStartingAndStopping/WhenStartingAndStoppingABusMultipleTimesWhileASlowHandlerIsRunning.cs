using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    public class WhenStartingAndStoppingABusMultipleTimesWhileASlowHandlerIsRunning : TestForBus
    {
        protected override async Task When()
        {
            await Bus.Stop();

            await Bus.Start();
            await Bus.Send(new SlowCommand());
            await Bus.Stop();

            await Bus.Start();
            await Bus.Stop();
        }

        [Test]
        public void NothingShouldGoBang()
        {
        }
    }
}