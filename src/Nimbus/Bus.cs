using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.PoisonMessages;

namespace Nimbus
{
    public class Bus : IBus, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICommandSender _commandSender;
        private readonly IRequestSender _requestSender;
        private readonly IMulticastRequestSender _multicastRequestSender;
        private readonly IEventSender _eventSender;
        private readonly IMessagePump[] _messagePumps;
        private readonly IDeadLetterQueues _deadLetterQueues;

        internal Bus(ILogger logger,
                     ICommandSender commandSender,
                     IRequestSender requestSender,
                     IMulticastRequestSender multicastRequestSender,
                     IEventSender eventSender,
                     IEnumerable<IMessagePump> messagePumps,
                     IDeadLetterQueues deadLetterQueues)
        {
            _logger = logger;
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

        public Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset deliveryTime) where TBusCommand : IBusCommand
        {
            return Task.Run(() => _commandSender.SendAt(busCommand, deliveryTime));
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
            _logger.Debug("Bus starting...");

            var messagePumpStartTasks = _messagePumps.Select(p => Task.Run(async () => await p.Start())).ToArray();

            try
            {
                Task.WaitAll(messagePumpStartTasks);
            }
            catch (AggregateException aex)
            {
                _logger.Error(aex, "Failed to start bus.");
                throw new BusException("Failed to start bus", aex);
            }

            _logger.Info("Bus started.");
        }

        public void Stop()
        {
            _logger.Debug("Bus stopping...");

            var messagePumpStopTasks = _messagePumps.Select(p => Task.Run(async () => await p.Stop())).ToArray();

            try
            {
                Task.WaitAll(messagePumpStopTasks);
            }
            catch (AggregateException aex)
            {
                throw new BusException("Failed to stop bus", aex);
            }

            _logger.Info("Bus stopped.");
        }

        public EventHandler<EventArgs> Disposing;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Stop();

            var handler = Disposing;
            if (handler == null) return;

            handler(this, EventArgs.Empty);
        }
    }
}