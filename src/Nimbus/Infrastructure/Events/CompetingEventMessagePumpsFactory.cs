using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.Events
{
    internal class CompetingEventMessagePumpsFactory : ICreateComponents
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly CompetingEventHandlerTypesSetting _competingEventHandlerTypes;
        private readonly ICompetingEventHandlerFactory _competingEventHandlerFactory;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public CompetingEventMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                 CompetingEventHandlerTypesSetting competingEventHandlerTypes,
                                                 ICompetingEventHandlerFactory competingEventHandlerFactory,
                                                 ILogger logger,
                                                 INimbusMessagingFactory messagingFactory,
                                                 IClock clock)
        {
            _applicationName = applicationName;
            _competingEventHandlerTypes = competingEventHandlerTypes;
            _competingEventHandlerFactory = competingEventHandlerFactory;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _clock = clock;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating competing event message pumps");

            var eventTypes = _competingEventHandlerTypes.Value.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleCompetingEvent<>)))
                                                        .Select(gi => gi.GetGenericArguments().Single())
                                                        .OrderBy(t => t.FullName)
                                                        .Distinct()
                                                        .ToArray();

            foreach (var eventType in eventTypes)
            {
                _logger.Debug("Registering Message Pump for Competing Event type {0}", eventType.Name);

                var topicPath = PathFactory.TopicPathFor(eventType);
                var subscriptionName = String.Format("{0}", _applicationName);
                var receiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);

                var dispatcher = new CompetingEventMessageDispatcher(_competingEventHandlerFactory, eventType);
                _garbageMan.Add(dispatcher);

                var pump = new MessagePump(receiver, dispatcher, _logger, _clock);
                _garbageMan.Add(pump);

                yield return pump;
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