using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly IClock _clock;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public RequestMessagePumpsFactory(IClock clock,
                                          ILogger logger,
                                          IMessageDispatcherFactory messageDispatcherFactory,
                                          INimbusMessagingFactory messagingFactory,
                                          IRouter router,
                                          ITypeProvider typeProvider)
        {
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _clock = clock;
            _typeProvider = typeProvider;
            _messagingFactory = messagingFactory;
            _router = router;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _typeProvider.RequestHandlerTypes)
            {
                var requestTypes = handlerType.GetGenericInterfacesClosing(typeof (IHandleRequest<,>))
                                              .Select(gi => gi.GetGenericArguments().First())
                                              .OrderBy(t => t.FullName)
                                              .Distinct()
                                              .ToArray();

                foreach (var requestType in requestTypes)
                {
                    var queuePath = _router.Route(requestType);
                    
                    _logger.Debug("Creating message pump for request queue {0}", queuePath);
                    var messageReceiver = _messagingFactory.GetQueueReceiver(queuePath);
                    var handlerMap = new Dictionary<Type, Type> { { requestType, handlerType } };
                    var pump = new MessagePump(_clock, _logger, _messageDispatcherFactory.Create(typeof (IHandleRequest<,>), handlerMap), messageReceiver);
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