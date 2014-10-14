using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : ICreateComponents
    {
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILogger _logger;
        private readonly IHandlerMapper _handlerMapper;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly NimbusTaskFactory _taskFactory;

        public CommandMessagePumpsFactory(IClock clock,
                                          IDispatchContextManager dispatchContextManager,
                                          IHandlerMapper handlerMapper,
                                          ILogger logger,
                                          IMessageDispatcherFactory messageDispatcherFactory,
                                          INimbusMessagingFactory messagingFactory,
                                          IRouter router,
                                          ITypeProvider typeProvider,
                                          NimbusTaskFactory taskFactory)
        {
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _handlerMapper = handlerMapper;
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _messagingFactory = messagingFactory;
            _router = router;
            _typeProvider = typeProvider;
            _taskFactory = taskFactory;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            var openGenericHandlerType = typeof (IHandleCommand<>);
            var handlerTypes = _typeProvider.CommandHandlerTypes.ToArray();

            // Create a single connection to each command queue determined by routing
            var allMessageTypesHandledByThisEndpoint = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, handlerTypes);
            var bindings = allMessageTypesHandledByThisEndpoint
                .Select(m => new {MessageType = m, QueuePath = _router.Route(m, QueueOrTopic.Queue)})
                .GroupBy(b => b.QueuePath)
                .Select(g => new {QueuePath = g.Key, HandlerTypes = g.SelectMany(x => _handlerMapper.GetHandlerTypesFor(openGenericHandlerType, x.MessageType))});

            // Each binding to a queue can handle one or more command types depending on the routes that are defined
            foreach (var binding in bindings)
            {
                var messageTypes = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, binding.HandlerTypes).ToArray();

                _logger.Debug("Creating message pump for command queue '{0}' handling {1}", binding.QueuePath, messageTypes.ToTypeNameSummary(selector: t => t.Name));
                var messageReceiver = _messagingFactory.GetQueueReceiver(binding.QueuePath);

                var handlerMap = _handlerMapper.GetHandlerMapFor(openGenericHandlerType, messageTypes);
                var pump = new MessagePump(_clock,
                                           _dispatchContextManager,
                                           _logger,
                                           _messageDispatcherFactory.Create(openGenericHandlerType, handlerMap),
                                           messageReceiver,
                                           _taskFactory);
                _garbageMan.Add(pump);

                yield return pump;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _garbageMan.Dispose();
        }
    }
}