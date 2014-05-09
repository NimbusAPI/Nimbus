using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;

namespace Nimbus.Configuration
{
    internal class MessagePumpsManager : IMessagePumpsManager
    {
        private readonly IMessagePump _responseMessagePump;
        private readonly IEnumerable<IMessagePump> _requestMessagePumps;
        private readonly IEnumerable<IMessagePump> _commandMessagePumps;
        private readonly IEnumerable<IMessagePump> _multicastEventMessagePumps;
        private readonly IEnumerable<IMessagePump> _competingEventMessagePumps;
        private readonly IEnumerable<IMessagePump> _multicastRequestMessagePumps;

        public MessagePumpsManager(IMessagePump responseMessagePump,
                                   IEnumerable<IMessagePump> requestMessagePumps,
                                   IEnumerable<IMessagePump> commandMessagePumps,
                                   IEnumerable<IMessagePump> multicastRequestMessagePumps,
                                   IEnumerable<IMessagePump> multicastEventMessagePumps,
                                   IEnumerable<IMessagePump> competingEventMessagePumps)
        {
            _responseMessagePump = responseMessagePump;
            _commandMessagePumps = commandMessagePumps;
            _requestMessagePumps = requestMessagePumps;
            _multicastRequestMessagePumps = multicastRequestMessagePumps;
            _multicastEventMessagePumps = multicastEventMessagePumps;
            _competingEventMessagePumps = competingEventMessagePumps;
        }

        public async Task Start(MessagePumpTypes messagePumpTypes)
        {
            await DoForAllPumps(messagePumpTypes, pump => pump.Start());
        }

        public async Task Stop(MessagePumpTypes messagePumpTypes)
        {
            await DoForAllPumps(messagePumpTypes, pump => pump.Stop());
        }

        private async Task DoForAllPumps(MessagePumpTypes waitForPumpTypes, Func<IMessagePump, Task> action)
        {
            var typesToProcessInBackground = (MessagePumpTypes)((int)waitForPumpTypes ^ -1);

            var messagePumpsToWaitFor = GetMessagePumps(waitForPumpTypes).ToArray();
            var messagePumpsToHandleInBackground = GetMessagePumps(typesToProcessInBackground).ToArray();

            await messagePumpsToWaitFor
                .Select(pump => Task.Run(async () => await action(pump)))
                .WhenAll();

#pragma warning disable 4014
            Task.Run(async () =>
                           {
                               // pause for a tiny bit here so that if people want messages on the bus immediately then their
                               // _bus.Send/Whatever(...) call can get into the threadpool queue before we flood it with potentially
                               // thousands of other message pump creation tasks.
                               await Task.Delay(100);   

                               await messagePumpsToHandleInBackground
                                   .Select(pump => Task.Run(async () => await pump.Start()))
                                   .WhenAll();
                           });
#pragma warning restore 4014
        }

        private IEnumerable<IMessagePump> GetMessagePumps(MessagePumpTypes messagePumpTypes)
        {
            if (messagePumpTypes.HasFlag(MessagePumpTypes.Response)) yield return _responseMessagePump;
            if (messagePumpTypes.HasFlag(MessagePumpTypes.Request)) foreach (var pump in _requestMessagePumps) yield return pump;
            if (messagePumpTypes.HasFlag(MessagePumpTypes.Command)) foreach (var pump in _commandMessagePumps) yield return pump;
            if (messagePumpTypes.HasFlag(MessagePumpTypes.MulticastRequest)) foreach (var pump in _multicastRequestMessagePumps) yield return pump;
            if (messagePumpTypes.HasFlag(MessagePumpTypes.MulticastEvent)) foreach (var pump in _multicastEventMessagePumps) yield return pump;
            if (messagePumpTypes.HasFlag(MessagePumpTypes.CompetingEvent)) foreach (var pump in _competingEventMessagePumps) yield return pump;
        }
    }

}