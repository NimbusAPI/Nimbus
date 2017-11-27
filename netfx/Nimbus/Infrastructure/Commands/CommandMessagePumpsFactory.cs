using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : MessagePumpFactory
    {
        private readonly ILogger _logger;
        private readonly IHandlerMapper _handlerMapper;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly INimbusTransport _transport;
        private readonly IRouter _router;
        private readonly ITypeProvider _typeProvider;
        private readonly PoorMansIoC _container;
        private readonly IPathFactory _pathFactory;

        public CommandMessagePumpsFactory(IHandlerMapper handlerMapper,
                                          ILogger logger,
                                          IMessageDispatcherFactory messageDispatcherFactory,
                                          INimbusTransport transport,
                                          IPathFactory pathFactory,
                                          IRouter router,
                                          ITypeProvider typeProvider,
                                          PoorMansIoC container)
        {
            _handlerMapper = handlerMapper;
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _transport = transport;
            _router = router;
            _typeProvider = typeProvider;
            _container = container;
            _pathFactory = pathFactory;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            var openGenericHandlerType = typeof(IHandleCommand<>);
            var handlerTypes = _typeProvider.CommandHandlerTypes.ToArray();

            // Create a single connection to each command queue determined by routing
            var allMessageTypesHandledByThisEndpoint = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, handlerTypes);
            var bindings = allMessageTypesHandledByThisEndpoint
                .Select(m => new {MessageType = m, QueuePath = _router.Route(m, QueueOrTopic.Queue, _pathFactory)})
                .GroupBy(b => b.QueuePath)
                .Select(g => new {QueuePath = g.Key, HandlerTypes = g.SelectMany(x => _handlerMapper.GetHandlerTypesFor(openGenericHandlerType, x.MessageType))});

            // Each binding to a queue can handle one or more command types depending on the routes that are defined
            foreach (var binding in bindings)
            {
                var messageTypes = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, binding.HandlerTypes).ToArray();

                _logger.Debug("Creating message pump for command queue '{0}' handling {1}", binding.QueuePath, messageTypes.ToTypeNameSummary(selector: t => t.Name));

                var messageReceiver = _transport.GetQueueReceiver(binding.QueuePath);
                var handlerMap = _handlerMapper.GetHandlerMapFor(openGenericHandlerType, messageTypes);
                var messageDispatcher = _messageDispatcherFactory.Create(openGenericHandlerType, handlerMap);

                var pump = _container.ResolveWithOverrides<MessagePump>(messageReceiver, messageDispatcher);
                GarbageMan.Add(pump);
                yield return pump;
            }
        }
    }
}