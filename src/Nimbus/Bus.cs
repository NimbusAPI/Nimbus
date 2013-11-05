using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly ICommandSender _commandSender;
        private readonly ITimeoutSender _timeoutSender;
        private readonly IRequestSender _requestSender;
        private readonly IEventSender _eventSender;
        private readonly IMessagePump[] _messagePumps;
        private readonly IDeadLetterQueues _deadLetterQueues;

        public Bus(ICommandSender commandSender, ITimeoutSender timeoutSender, IRequestSender requestSender, IEventSender eventSender, IEnumerable<IMessagePump> messagePumps, IDeadLetterQueues deadLetterQueues)
        {
            _commandSender = commandSender;
            _timeoutSender = timeoutSender;
            _requestSender = requestSender;
            _eventSender = eventSender;
            _deadLetterQueues = deadLetterQueues;
            _messagePumps = messagePumps.ToArray();
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand)
        {
            await _commandSender.Send(busCommand);
        }

        public async Task Defer<TBusCommand>(DateTime proccessAt, TBusCommand busCommand)
        {
            await _timeoutSender.Defer(proccessAt, busCommand);
        }
        
        public async Task Defer<TBusCommand>(TimeSpan delay, TBusCommand busCommand)
        {
            await _timeoutSender.Defer(delay, busCommand);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var response = await _requestSender.SendRequest(busRequest);
            return response;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent)
        {
            await _eventSender.Publish(busEvent);
        }

        public IDeadLetterQueues DeadLetterQueues
        {
            get { return _deadLetterQueues; }
        }

        public void Start()
        {
            foreach (var pump in _messagePumps) pump.Start();
        }

        public void Stop()
        {
            foreach (var messagePump in _messagePumps) messagePump.Stop();
        }
    }
}