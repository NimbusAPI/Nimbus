using System.Threading;
using DevHarness.Messages;
using Nimbus.InfrastructureContracts;

namespace DevHarness.Harnesses
{
    public class CommandSender
    {
        private readonly IBus _bus;

        public CommandSender(IBus bus)
        {
            _bus = bus;
        }

        public void Run()
        {
            for (int i = 0; i < 20; i++)
            {
                SendMessage();
                Thread.Sleep(10000);
            }
        }

        private void SendMessage()
        {
            var test = new TestCommand
                       {
                           Id = 4,
                           Name = "Mocking Spongebob"
                       };
            _bus.Send(test);
        }
    }
}