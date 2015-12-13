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
        private readonly IMessagePump[] _requestMessagePumps;
        private readonly IMessagePump[] _commandMessagePumps;
        private readonly IMessagePump[] _multicastEventMessagePumps;
        private readonly IMessagePump[] _competingEventMessagePumps;
        private readonly IMessagePump[] _multicastRequestMessagePumps;

        public MessagePumpsManager(IMessagePump responseMessagePump,
                                   IEnumerable<IMessagePump> requestMessagePumps,
                                   IEnumerable<IMessagePump> commandMessagePumps,
                                   IEnumerable<IMessagePump> multicastRequestMessagePumps,
                                   IEnumerable<IMessagePump> multicastEventMessagePumps,
                                   IEnumerable<IMessagePump> competingEventMessagePumps)
        {
            _responseMessagePump = responseMessagePump;
            _commandMessagePumps = commandMessagePumps.ToArray();
            _requestMessagePumps = requestMessagePumps.ToArray();
            _multicastRequestMessagePumps = multicastRequestMessagePumps.ToArray();
            _multicastEventMessagePumps = multicastEventMessagePumps.ToArray();
            _competingEventMessagePumps = competingEventMessagePumps.ToArray();
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
            var typesToProcessInBackground = (MessagePumpTypes) ((int) waitForPumpTypes ^ -1);

            var messagePumpsToWaitFor = GetMessagePumps(waitForPumpTypes).ToArray();
            var messagePumpsToHandleInBackground = GetMessagePumps(typesToProcessInBackground).ToArray();

            await messagePumpsToWaitFor
                .Select(action)
                .WhenAll();

#pragma warning disable 4014
            Task.Run(async () =>
                           {
                               // pause for a tiny bit here so that if people want messages on the bus immediately then their
                               // _bus.Send/Whatever(...) call can get into the threadpool queue before we flood it with potentially
                               // thousands of other message pump creation tasks.
                               await Task.Delay(100);

                               await messagePumpsToHandleInBackground
                                   .Select(action)
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