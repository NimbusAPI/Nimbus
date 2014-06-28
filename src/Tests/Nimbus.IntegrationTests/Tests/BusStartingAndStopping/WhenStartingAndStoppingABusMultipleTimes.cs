using System.Threading.Tasks;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping
{
    [TestFixture]
    public class WhenStartingAndStoppingABusMultipleTimes : TestForBus
    {
        protected override async Task When()
        {
            await Bus.Stop();
            await Bus.Start();
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