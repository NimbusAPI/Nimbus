using System;
using Nimbus.MessageContracts;

namespace Nimbus.UnitTests.DispatcherTests.MessageContracts
{
    public class FooCommand : IBusCommand
    {
        public Guid Id { get; set; }

        public FooCommand()
        {
        }

        public FooCommand(Guid id)
        {
            Id = id;
        }
    }
}