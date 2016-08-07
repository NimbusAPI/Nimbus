using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat;
using Nimbus.Infrastructure.RequestResponse;
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
        private readonly IHeartbeat _heartbeat;

        private readonly object _mutex = new object();
        private bool _isRunning;

        internal Bus(ILogger logger,
                     ICommandSender commandSender,
                     IRequestSender requestSender,
                     IMulticastRequestSender multicastRequestSender,
                     IEventSender eventSender,
                     IMessagePumpsManager messagePumpsManager,
                     IDeadLetterOffice deadLetterOffice,
                     IHeartbeat heartbeat)
        {
            _logger = logger;
            _commandSender = commandSender;
            _requestSender = requestSender;
            _multicastRequestSender = multicastRequestSender;
            _eventSender = eventSender;
            _heartbeat = heartbeat;
            _messagePumpsManager = messagePumpsManager;
            DeadLetterOffice = deadLetterOffice;

            InstanceId = Guid.NewGuid();

            Started += async delegate { await _heartbeat.Start(); };
            Stopping += async delegate { await _heartbeat.Stop(); };
        }

        public Guid InstanceId { get; }

        public Task Send<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            return Task.Run(() => _commandSender.Send(busCommand)).ConfigureAwaitFalse();
        }

        public Task SendAt<TBusCommand>(TBusCommand busCommand, DateTimeOffset deliveryTime) where TBusCommand : IBusCommand
        {
            return Task.Run(() => _commandSender.SendAt(busCommand, deliveryTime)).ConfigureAwaitFalse();
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return Task.Run(() => _requestSender.SendRequest(busRequest)).ConfigureAwaitFalse();
        }

        public Task<TResponse> Request<TRequest, TResponse>(IBusRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusRequest<TRequest, TResponse>
            where TResponse : IBusResponse
        {
            return Task.Run(() => _requestSender.SendRequest(busRequest, timeout)).ConfigureAwaitFalse();
        }

        public Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IBusMulticastRequest<TRequest, TResponse> busRequest, TimeSpan timeout)
            where TRequest : IBusMulticastRequest<TRequest, TResponse>
            where TResponse : IBusMulticastResponse
        {
            return Task.Run(() => _multicastRequestSender.SendRequest(busRequest, timeout)).ConfigureAwaitFalse();
        }

        public Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            return Task.Run(() => _eventSender.Publish(busEvent)).ConfigureAwaitFalse();
        }

        public IDeadLetterOffice DeadLetterOffice { get; }

        public EventHandler<EventArgs> Starting;
        public EventHandler<EventArgs> Started;
        public EventHandler<EventArgs> Stopping;
        public EventHandler<EventArgs> Stopped;

        public Task Start(MessagePumpTypes messagePumpTypes = MessagePumpTypes.Default)
        {
            return Task.Run(async () =>
                                  {
                                      lock (_mutex)
                                      {
                                          if (_isRunning) return;
                                          _isRunning = true;
                                      }

                                      _logger.Debug("Bus starting...");
                                      var sw = Stopwatch.StartNew();

                                      try
                                      {
                                          var startingHandler = Starting;
                                          startingHandler?.Invoke(this, EventArgs.Empty);

                                          await _messagePumpsManager.Start(messagePumpTypes);
                                      }
                                      catch (AggregateException aex)
                                      {
                                          _logger.Error(aex, "Failed to start bus.");
                                          throw new BusException("Failed to start bus", aex);
                                      }

                                      var startedHandler = Started;
                                      startedHandler?.Invoke(this, EventArgs.Empty);

                                      sw.Stop();
                                      _logger.Info("Bus started in {Elapsed}.", sw.Elapsed);
                                  }).ConfigureAwaitFalse();
        }

        public Task Stop(MessagePumpTypes messagePumpTypes = MessagePumpTypes.All)
        {
            return Task.Run(async () =>
                                  {
                                      lock (_mutex)
                                      {
                                          if (!_isRunning) return;
                                          _isRunning = false;
                                      }

                                      _logger.Debug("Bus stopping...");
                                      var sw = Stopwatch.StartNew();

                                      try
                                      {
                                          var stoppingHandler = Stopping;
                                          stoppingHandler?.Invoke(this, EventArgs.Empty);

                                          await _messagePumpsManager.Stop(messagePumpTypes);
                                      }
                                      catch (AggregateException aex)
                                      {
                                          throw new BusException("Failed to stop bus", aex);
                                      }

                                      var stoppedHandler = Stopped;
                                      stoppedHandler?.Invoke(this, EventArgs.Empty);

                                      sw.Stop();
                                      _logger.Info("Bus stopped in {Elapsed}.", sw.Elapsed);
                                  }).ConfigureAwaitFalse();
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
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}