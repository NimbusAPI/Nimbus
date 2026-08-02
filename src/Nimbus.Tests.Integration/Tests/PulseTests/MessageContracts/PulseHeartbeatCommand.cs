using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nimbus.Extensions.Pulse;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.PulseTests.MessageContracts
{
    public class PulseHeartbeatCommand : IBusCommand, IPulseMessage
    {
        public DateTimeOffset PulseTime { get; set; }
    }

    public class PulseHeartbeatCommandHandler : IHandleCommand<PulseHeartbeatCommand>
    {
        /// <summary>
        ///     Every instance in the test shares this, so a schedule that fired on more than one of them
        ///     shows up as the same PulseTime appearing twice.
        /// </summary>
        public static readonly ConcurrentBag<DateTimeOffset> FiredOccurrences = new ConcurrentBag<DateTimeOffset>();

        public Task Handle(PulseHeartbeatCommand busCommand)
        {
            FiredOccurrences.Add(busCommand.PulseTime);
            return Task.CompletedTask;
        }
    }
}
