using System;
using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.MessageContracts
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