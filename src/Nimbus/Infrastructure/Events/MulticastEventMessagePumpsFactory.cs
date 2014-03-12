using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessagePumpsFactory : ICreateComponents
    {
        private readonly IQueueManager _queueManager;
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly MulticastEventHandlerTypesSetting _multicastEventHandlerTypes;
        private readonly ILogger _logger;
        private readonly IMulticastEventHandlerFactory _multicastEventHandlerFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        internal MulticastEventMessagePumpsFactory(IQueueManager queueManager,
                                                   ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   MulticastEventHandlerTypesSetting multicastEventHandlerTypes,
                                                   ILogger logger,
                                                   IMulticastEventHandlerFactory multicastEventHandlerFactory,
                                                   IClock clock)
        {
            _queueManager = queueManager;
            _applicationName = applicationName;
            _instanceName = instanceName;
            _multicastEventHandlerTypes = multicastEventHandlerTypes;
            _logger = logger;
            _multicastEventHandlerFactory = multicastEventHandlerFactory;
            _clock = clock;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating multicast event message pumps");

            var eventTypes = _multicastEventHandlerTypes.Value
                                                        .SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleMulticastEvent<>)))
                                                        .Select(gi => gi.GetGenericArguments().Single())
                                                        .OrderBy(t => t.FullName)
                                                        .Distinct()
                                                        .ToArray();

            foreach (var eventType in eventTypes)
            {
                _logger.Debug("Creating message pump for multicast event type {0}", eventType.Name);

                var topicPath = PathFactory.TopicPathFor(eventType);
                var subscriptionName = String.Format("{0}.{1}", _applicationName, _instanceName);
                var receiver = new NimbusSubscriptionMessageReceiver(_queueManager, topicPath, subscriptionName);
                _garbageMan.Add(receiver);

                var dispatcher = new MulticastEventMessageDispatcher(_multicastEventHandlerFactory, eventType, _clock);
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