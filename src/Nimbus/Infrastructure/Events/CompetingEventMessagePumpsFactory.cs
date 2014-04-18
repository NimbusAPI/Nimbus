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
    internal class CompetingEventMessagePumpsFactory : ICreateComponents
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly CompetingEventHandlerTypesSetting _competingEventHandlerTypes;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CompetingEventMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                 CompetingEventHandlerTypesSetting competingEventHandlerTypes,
                                                 ILogger logger,
                                                 INimbusMessagingFactory messagingFactory,
                                                 IBrokeredMessageFactory brokeredMessageFactory,
                                                 IClock clock,
                                                 IDependencyResolver dependencyResolver)
        {
            _applicationName = applicationName;
            _competingEventHandlerTypes = competingEventHandlerTypes;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _competingEventHandlerTypes.Value)
            {
                var eventTypes = handlerType
                    .GetGenericInterfacesClosing(typeof (IHandleCompetingEvent<>))
                    .Select(gi => gi.GetGenericArguments().Single())
                    .ToArray();

                foreach (var eventType in eventTypes)
                {
                    var topicPath = PathFactory.TopicPathFor(eventType);
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, handlerType);

                    _logger.Debug("Creating message pump for competing event {0}/{1}", topicPath, subscriptionName);

                    var receiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);

                    var dispatcher = new CompetingEventMessageDispatcher(_dependencyResolver, _brokeredMessageFactory, handlerType, _clock, eventType);
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