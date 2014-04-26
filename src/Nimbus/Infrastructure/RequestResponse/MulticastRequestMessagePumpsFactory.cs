using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly ApplicationNameSetting _applicationName;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IClock _clock;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly IDependencyResolver _dependencyResolver;

        public MulticastRequestMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                   IBrokeredMessageFactory brokeredMessageFactory,
                                                   IClock clock,
                                                   IDependencyResolver dependencyResolver,
                                                   IInboundInterceptorFactory inboundInterceptorFactory,
                                                   ILogger logger,
                                                   INimbusMessagingFactory messagingFactory,
                                                   ITypeProvider typeProvider)
        {
            _logger = logger;
            _applicationName = applicationName;
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _typeProvider = typeProvider;
            _inboundInterceptorFactory = inboundInterceptorFactory;
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
                    var topicPath = PathFactory.TopicPathFor(requestType);
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, handlerType);
                    _logger.Debug("Creating message pump for multicast request {0}/{1}", topicPath, subscriptionName);

                    var messageReceiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);

                    var dispatcher = new RequestMessageDispatcher(_messagingFactory,
                                                                  _brokeredMessageFactory,
                                                                  _inboundInterceptorFactory,
                                                                  requestType,
                                                                  _clock,
                                                                  _logger,
                                                                  _dependencyResolver,
                                                                  handlerType);
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