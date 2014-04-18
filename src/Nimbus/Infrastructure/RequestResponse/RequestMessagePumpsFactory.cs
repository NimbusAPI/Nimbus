using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public RequestMessagePumpsFactory(ILogger logger,
                                          INimbusMessagingFactory messagingFactory,
                                          IBrokeredMessageFactory brokeredMessageFactory,
                                          IClock clock,
                                          IDependencyResolver dependencyResolver,
                                          ITypeProvider typeProvider)
        {
            _logger = logger;
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _typeProvider = typeProvider;
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
                    var queuePath = PathFactory.QueuePathFor(requestType);
                    _logger.Debug("Creating message pump for request queue {0}", queuePath);

                    var messageReceiver = _messagingFactory.GetQueueReceiver(queuePath);

                    var dispatcher = new RequestMessageDispatcher(_messagingFactory, _brokeredMessageFactory, requestType, _clock, _logger, _dependencyResolver, handlerType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(messageReceiver, dispatcher, _logger, _clock);
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