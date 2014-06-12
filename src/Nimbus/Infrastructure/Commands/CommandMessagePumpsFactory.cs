using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : ICreateComponents
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CommandMessagePumpsFactory(IClock clock,
                                          ILogger logger,
                                          IMessageDispatcherFactory messageDispatcherFactory,
                                          INimbusMessagingFactory messagingFactory,
                                          IRouter router,
                                          ITypeProvider typeProvider)
        {
            _clock = clock;
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _messagingFactory = messagingFactory;
            _router = router;
            _typeProvider = typeProvider;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            var handlerTypes = _typeProvider.CommandHandlerTypes.ToArray();

            foreach (var handlerType in handlerTypes)
            {
                var commandTypes = handlerType.GetGenericInterfacesClosing(typeof (IHandleCommand<>)).Select(gi => gi.GetGenericArguments().First());

                foreach (var commandType in commandTypes)
                {
                    var queuePath = _router.Route(commandType);

                    _logger.Debug("Creating message pump for {0}", queuePath);
                    var messageReceiver = _messagingFactory.GetQueueReceiver(queuePath);
                    var handlerMap = new Dictionary<Type, Type>{{commandType, handlerType}};
                    var pump = new MessagePump(_clock, _logger, _messageDispatcherFactory.Create(typeof (IHandleCommand<>), handlerMap), messageReceiver);
                    _garbageMan.Add(pump);

                    yield return pump;
                }
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