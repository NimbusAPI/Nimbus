using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus
{
    public static class BatchSendingExtensions
    {
        public static Task SendAll(this IBus bus, IEnumerable<IBusCommand> busCommands)
        {
            return Task.Run(async () =>
                                  {
                                      foreach (var c in busCommands)
                                      {
                                          await bus.Send(c);
                                      }
                                  }).ConfigureAwaitFalse();
        }

        public static Task PublishAll(this IBus bus, IEnumerable<IBusEvent> busEvents)
        {
            return Task.Run(async () =>
                                  {
                                      foreach (var e in busEvents)
                                      {
                                          await bus.Publish(e);
                                      }
                                  }).ConfigureAwaitFalse();
        }
    }
}