using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public static class BatchSendingExtensions
    {
        public static Task SendAll(this IBus bus, IEnumerable<IBusCommand> busCommands)
        {
            return Task.Run(async () => { await Task.WhenAll(busCommands.Select(e => bus.Send(e))); });
        }

        public static Task PublishAll(this IBus bus, IEnumerable<IBusEvent> busEvents)
        {
            return Task.Run(async () => { await Task.WhenAll(busEvents.Select(e => bus.Publish(e))); });
        }
    }
}