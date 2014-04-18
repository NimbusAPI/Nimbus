using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessagePumpsFactory : ICreateComponents
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly MulticastEventHandlerTypesSetting _multicastEventHandlerTypes;
        private readonly ILogger _logger;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly INimbusMessagingFactory _messagingFactory;

        internal MulticastEventMessagePumpsFactory(IDependencyResolver dependencyResolver,
                                                   ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   MulticastEventHandlerTypesSetting multicastEventHandlerTypes,
                                                   ILogger logger,
                                                   IBrokeredMessageFactory brokeredMessageFactory,
                                                   IClock clock,
                                                   INimbusMessagingFactory messagingFactory)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _multicastEventHandlerTypes = multicastEventHandlerTypes;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _messagingFactory = messagingFactory;
            _dependencyResolver = dependencyResolver;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _multicastEventHandlerTypes.Value)
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

                    var dispatcher = new MulticastEventMessageDispatcher(_dependencyResolver, _brokeredMessageFactory, handlerType, _clock, eventType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(receiver, dispatcher, _logger, _clock);
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