﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly IHandlerMapper _handlerMapper;
        private readonly ITypeProvider _typeProvider;
        private readonly IPathFactory _pathFactory;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly INimbusTaskFactory _taskFactory;

        public RequestMessagePumpsFactory(IClock clock,
                                          IDispatchContextManager dispatchContextManager,
                                          IHandlerMapper handlerMapper,
                                          ILogger logger,
                                          IMessageDispatcherFactory messageDispatcherFactory,
                                          INimbusMessagingFactory messagingFactory,
                                          INimbusTaskFactory taskFactory,
                                          IRouter router,
                                          ITypeProvider typeProvider,
                                          IPathFactory pathFactory)
        {
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _handlerMapper = handlerMapper;
            _typeProvider = typeProvider;
            _pathFactory = pathFactory;
            _taskFactory = taskFactory;
            _messagingFactory = messagingFactory;
            _router = router;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            var openGenericHandlerType = typeof (IHandleRequest<,>);
            var handlerTypes = _typeProvider.RequestHandlerTypes.ToArray();

            // Create a single connection to each request queue determined by routing
            var allMessageTypesHandledByThisEndpoint = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, handlerTypes);
            var bindings = allMessageTypesHandledByThisEndpoint
                .Select(m => new {MessageType = m, QueuePath = _router.Route(m, QueueOrTopic.Queue, _pathFactory)})
                .GroupBy(b => b.QueuePath)
                .Select(g => new {QueuePath = g.Key, HandlerTypes = g.SelectMany(x => _handlerMapper.GetHandlerTypesFor(openGenericHandlerType, x.MessageType))});

            // Each binding to a queue can handle one or more request types depending on the routes that are defined
            foreach (var binding in bindings)
            {
                var messageTypes = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, binding.HandlerTypes).ToArray();

                _logger.Debug("Creating message pump for request queue '{0}' handling {1}", binding.QueuePath, messageTypes.ToTypeNameSummary(selector: t => t.Name));
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