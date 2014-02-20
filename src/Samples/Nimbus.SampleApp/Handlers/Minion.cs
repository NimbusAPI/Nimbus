using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class Minion : IHandleCommand<JustDoIt>
    {
        public async Task Handle(JustDoIt busCommand)
        {
            Console.WriteLine("Yes boss");
        }
    }
}