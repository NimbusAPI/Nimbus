using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
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