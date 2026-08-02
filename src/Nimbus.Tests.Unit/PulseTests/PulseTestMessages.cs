using System;
using Nimbus.Extensions.Pulse;
using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.PulseTests
{
    public class SomePulseCommand : IBusCommand
    {
        public string Label { get; set; }
        public string[] Tags { get; set; }
    }

    public class SomePulseEvent : IBusEvent
    {
    }

    public class SomePulseCommandThatKnowsItsPulseTime : IBusCommand, IPulseMessage
    {
        public DateTimeOffset PulseTime { get; set; }
        public string Label { get; set; }
    }

    public class SomethingThatIsNotAMessage
    {
    }
}
