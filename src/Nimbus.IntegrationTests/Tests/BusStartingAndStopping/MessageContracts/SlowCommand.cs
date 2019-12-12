using System;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.BusStartingAndStopping.MessageContracts
{
    public class SlowCommand : IBusCommand
    {
        public Guid SomeId { get; set; }

        public SlowCommand()
        {
        }

        public SlowCommand(Guid someId)
        {
            SomeId = someId;
        }
    }
}