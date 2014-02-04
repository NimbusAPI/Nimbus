using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class MulticastEventMessagePumpsFactory
    {
        private readonly IQueueManager _queueManager;
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly CompetingEventHandlerTypesSetting _competingEventHandlerTypes;
        private readonly ILogger _logger;
        private readonly IMulticastEventBroker _multicastEventBroker;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;

        internal MulticastEventMessagePumpsFactory(IQueueManager queueManager,
                                                   ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   CompetingEventHandlerTypesSetting competingEventHandlerTypes,
                                                   ILogger logger,
                                                   IMulticastEventBroker multicastEventBroker,
                                                   DefaultBatchSizeSetting defaultBatchSize)
        {
            _queueManager = queueManager;
            _applicationName = applicationName;
            _instanceName = instanceName;
            _competingEventHandlerTypes = competingEventHandlerTypes;
            _logger = logger;
            _multicastEventBroker = multicastEventBroker;
            _defaultBatchSize = defaultBatchSize;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating competing event message pumps");

            var eventTypes = _competingEventHandlerTypes.Value
                                                        .SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleCompetingEvent<>)))
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
                var dispatcher = new MulticastEventMessageDispatcher(_multicastEventBroker, eventType);

                var pump = new MessagePump(receiver, dispatcher, _logger, _defaultBatchSize);
                yield return pump;
            }
        }
    }
}