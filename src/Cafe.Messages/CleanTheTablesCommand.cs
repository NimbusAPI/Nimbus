using System;
using Nimbus.Extensions.Pulse;
using Nimbus.MessageContracts;

namespace Cafe.Messages;

public class CleanTheTablesCommand : IBusCommand, IPulseMessage
{
    public DateTimeOffset PulseTime { get; set; }
}