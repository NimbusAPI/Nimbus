using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus
{
    public class Bus : IBus, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICommandSender _commandSender;
        private readonly IRequestSender _requestSender;
        private readonly IMulticastRequestSender _multicastRequestSender;
        private readonly IEventSender _eventSender;
        private readonly IMessagePumpsManager _messagePumpsManager;
        private readonly IDeadLetterQueues _deadLetterQueues;
        private readonly INimbusTaskFactory _taskFactory;
        private readonly IHeartbeat _heartbeat;

        private readonly object _mutex = new object();
        private bool _isRunning;

        internal Bus(ILogger logger,
                     ICommandSender commandSender,
                     IRequestSender requestSender,
                     IMulticastRequestSender multicastRequestSender,
                     IEventSender eventSender,
                     IMessagePumpsManager messagePumpsManager,
                     IDeadLetterQueues deadLetterQueues,
                     INimbusTaskFactory taskFactory,
                     IHeartbeat heartbeat)
        {
            _logger = logger;
            _commandSender = commandSender;
            _requestSender = requestSender;
            _multicastRequestSender = multicastRequestSender;
            _eventSender = eventSender;
            _deadLetterQueues = deadLetterQueues;
            _taskFactory = taskFactory;
            _heartbeat = heartbeat;
            _messagePumpsManager = messagePumpsManager;

            Started += async delegate { await _heartbeat.Start(); };
            Stopping += async delegate { await _heartbeat.Stop(); };
        }

        public Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            // We're explicitly invoking Task.Run in these facade methods to make sure that we break out of anyone else's
            // synchronisation context and run this stuff only on thread pool threads.  -andrewh 24/1/2014
            return _taskFactory.StartNew(() => _commandSender.Send(busCommand), TaskContext.Send).Unwrap();
        }

        public Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset deliveryTime) where TBusCommand : IBusCommand
        {
            return _taskFactory.StartNew(() => _commandSender.SendAt(busCommand, deliveryTime), TaskContext.Send).Unwrap();
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return _taskFactory.StartNew(() => _requestSender.SendRequest(busRequest), TaskContext.Send).Unwrap();
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return _taskFactory.StartNew(() => _requestSender.SendRequest(busRequest, timeout), TaskContext.Send).Unwrap();
        }

        public Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusMulticastRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusMulticastRequest<TRequest, TResponse>
            where TResponse : IBusMulticastResponse
        {
            return _taskFactory.StartNew(() => _multicastRequestSender.SendRequest(busRequest, timeout), TaskContext.Send).Unwrap();
        }

        public Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            return _taskFactory.StartNew(() => _eventSender.Publish(busEvent), TaskContext.Send).Unwrap();
        }

        public Task PublishAt<TBusEvent>(TBusEvent busEvent, DateTimeOffset deliveryTime) where TBusEvent : IBusEvent
        {
            return _taskFactory.StartNew(() => _eventSender.PublishAt(busEvent, deliveryTime), TaskContext.Send).Unwrap();
        }

        public IDeadLetterQueues DeadLetterQueues
        {
            get { return _deadLetterQueues; }
        }

        public EventHandler<EventArgs> Starting;
        public EventHandler<EventArgs> Started;
        public EventHandler<EventArgs> Stopping;
        public EventHandler<EventArgs> Stopped;

        public async Task Start(MessagePumpTypes messagePumpTypes = MessagePumpTypes.Default)
        {
            lock (_mutex)
            {
                if (_isRunning) return;
                _isRunning = true;
            }

            _logger.Debug("Bus starting...");

            try
            {
                var startingHandler = Starting;
                if (startingHandler != null) startingHandler(this, EventArgs.Empty);

                await _messagePumpsManager.Start(messagePumpTypes);
            }
            catch (AggregateException aex)
            {
                _logger.Error(aex, "Failed to start bus.");
                throw new BusException("Failed to start bus", aex);
            }

            var startedHandler = Started;
            if (startedHandler != null) startedHandler(this, EventArgs.Empty);

            _logger.Info("Bus started.");
        }

        public async Task Stop(MessagePumpTypes messagePumpTypes = MessagePumpTypes.All)
        {
            lock (_mutex)
            {
                if (!_isRunning) return;
                _isRunning = false;
            }

            _logger.Debug("Bus stopping...");

            try
            {
                var stoppingHandler = Stopping;
                if (stoppingHandler != null) stoppingHandler(this, EventArgs.Empty);

                await _messagePumpsManager.Stop(messagePumpTypes);
            }
            catch (AggregateException aex)
            {
                throw new BusException("Failed to stop bus", aex);
            }

            var stoppedHandler = Stopped;
            if (stoppedHandler != null) stoppedHandler(this, EventArgs.Empty);

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

            var handler = Disposing;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}