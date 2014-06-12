using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessagePumpsFactory : ICreateComponents
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly ILogger _logger;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly IClock _clock;
        private readonly ITypeProvider _typeProvider;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        internal MulticastEventMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   IClock clock,
                                                   ILogger logger,
                                                   IMessageDispatcherFactory messageDispatcherFactory,
                                                   INimbusMessagingFactory messagingFactory,
                                                   IRouter router,
                                                   ITypeProvider typeProvider)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _clock = clock;
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _messagingFactory = messagingFactory;
            _router = router;
            _typeProvider = typeProvider;
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
                    var topicPath = _router.Route(eventType);
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, _instanceName, handlerType);

                    _logger.Debug("Creating message pump for multicast event {0}/{1}", topicPath, subscriptionName);
                    var receiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);
                    var handlerMap = new Dictionary<Type, Type> { { eventType, handlerType } };
                    var pump = new MessagePump(_clock, _logger, _messageDispatcherFactory.Create(typeof(IHandleMulticastEvent<>), handlerMap), receiver);
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