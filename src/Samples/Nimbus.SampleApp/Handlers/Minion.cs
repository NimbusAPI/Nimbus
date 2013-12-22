using System;
using Nimbus.InfrastructureContracts;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp.Handlers
{
    public class Minion : IHandleCommand<JustDoIt>
    {
        public void Handle(JustDoIt busCommand)
        {
            Console.WriteLine("Yes boss");
        }
    }
}