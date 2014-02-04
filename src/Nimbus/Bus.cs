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

        public Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            // We're explicitly invoking Task.Run in these facade methods to make sure that we break out of anyone else's
            // synchronisation context and run this stuff only on thread pool threads.  -andrewh 24/1/2014
            return Task.Run(() => _commandSender.Send(busCommand));
        }

        public Task Send(IBusCommand busCommand)
        {
            // We're explicitly invoking Task.Run in these facade methods to make sure that we break out of anyone else's
            // synchronisation context and run this stuff only on thread pool threads.  -andrewh 24/1/2014
            return Task.Run(() => _commandSender.Send(busCommand));
        }

        public Task Defer<TBusCommand>(TimeSpan delay, TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            return Task.Run(() => _commandSender.SendAt(delay, busCommand));
        }

        public Task Defer<TBusCommand>(DateTimeOffset processAt, TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            return Task.Run(() => _commandSender.SendAt(processAt, busCommand));
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return Task.Run(() => _requestSender.SendRequest(busRequest));
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return Task.Run(() => _requestSender.SendRequest(busRequest, timeout));
        }

        public Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return Task.Run(() => _multicastRequestSender.SendRequest(busRequest, timeout));
        }

        public Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            return Task.Run(() => _eventSender.Publish(busEvent));
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