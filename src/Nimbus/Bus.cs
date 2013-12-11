using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Nimbus.PoisonMessages;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly ICommandSender _commandSender;
        private readonly IRequestSender _requestSender;
        private readonly IMulticastRequestSender _multicastRequestSender;
        private readonly IEventSender _eventSender;
        private readonly IMessagePump[] _messagePumps;
        private readonly IDeadLetterQueues _deadLetterQueues;

        internal Bus(ICommandSender commandSender,
                     IRequestSender requestSender,
                     IMulticastRequestSender multicastRequestSender,
                     IEventSender eventSender,
                     IEnumerable<IMessagePump> messagePumps,
                     IDeadLetterQueues deadLetterQueues)
        {
            _commandSender = commandSender;
            _requestSender = requestSender;
            _multicastRequestSender = multicastRequestSender;
            _eventSender = eventSender;
            _deadLetterQueues = deadLetterQueues;
            _messagePumps = messagePumps.ToArray();
        }

        public async Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            await _commandSender.Send(busCommand);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var response = await _requestSender.SendRequest(busRequest);
            return response;
        }

        public async Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var response = await _requestSender.SendRequest(busRequest, timeout);
            return response;
        }

        public async Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            var response = await _multicastRequestSender.SendRequest(busRequest, timeout);
            return response;
        }

        public async Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
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