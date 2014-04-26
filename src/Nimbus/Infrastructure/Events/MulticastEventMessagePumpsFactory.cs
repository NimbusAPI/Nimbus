using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessagePumpsFactory : ICreateComponents
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly ILogger _logger;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly ITypeProvider _typeProvider;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly INimbusMessagingFactory _messagingFactory;

        internal MulticastEventMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   IBrokeredMessageFactory brokeredMessageFactory,
                                                   IClock clock,
                                                   IDependencyResolver dependencyResolver,
                                                   IInboundInterceptorFactory inboundInterceptorFactory,
                                                   ILogger logger,
                                                   INimbusMessagingFactory messagingFactory,
                                                   ITypeProvider typeProvider)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _messagingFactory = messagingFactory;
            _typeProvider = typeProvider;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _dependencyResolver = dependencyResolver;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _typeProvider.MulticastEventHandlerTypes)
            {
                var eventTypes = handlerType
                    .GetGenericInterfacesClosing(typeof (IHandleMulticastEvent<>))
                    .Select(gi => gi.GetGenericArguments().Single())
                    .ToArray();

                foreach (var eventType in eventTypes)
                {
                    var topicPath = PathFactory.TopicPathFor(eventType);
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, _instanceName, handlerType);

                    _logger.Debug("Creating message pump for multicast event {0}/{1}", topicPath, subscriptionName);

                    var receiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);

                    var dispatcher = new MulticastEventMessageDispatcher(_dependencyResolver,
                                                                         _brokeredMessageFactory,
                                                                         _inboundInterceptorFactory,
                                                                         handlerType,
                                                                         _clock,
                                                                         eventType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(_clock, _logger, dispatcher, receiver);
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