using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessagePumpsFactory : ICreateComponents
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInterceptorFactory _interceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CommandMessagePumpsFactory(IBrokeredMessageFactory brokeredMessageFactory,
                                          IClock clock,
                                          IDependencyResolver dependencyResolver,
                                          IInterceptorFactory interceptorFactory,
                                          ILogger logger,
                                          INimbusMessagingFactory messagingFactory,
                                          ITypeProvider typeProvider)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _interceptorFactory = interceptorFactory;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _typeProvider = typeProvider;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _typeProvider.CommandHandlerTypes)
            {
                var commandTypes = handlerType.GetGenericInterfacesClosing(typeof (IHandleCommand<>)).Select(gi => gi.GetGenericArguments().First());

                foreach (var commandType in commandTypes)
                {
                    var queuePath = PathFactory.QueuePathFor(commandType);

                    _logger.Debug("Creating message pump for {0}", queuePath);

                    var messageReceiver = _messagingFactory.GetQueueReceiver(queuePath);

                    var dispatcher = new CommandMessageDispatcher(_dependencyResolver, _interceptorFactory, _brokeredMessageFactory, commandType, _clock, handlerType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(_clock, _logger, dispatcher, messageReceiver);
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