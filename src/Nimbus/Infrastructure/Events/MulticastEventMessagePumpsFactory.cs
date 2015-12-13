using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Events
{
    internal class MulticastEventMessagePumpsFactory : ICreateComponents
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly ILogger _logger;
        private readonly IMessageDispatcherFactory _messageDispatcherFactory;
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly IHandlerMapper _handlerMapper;
        private readonly ITypeProvider _typeProvider;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IRouter _router;
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttemptSetting;
        private readonly IDeadLetterOffice _deadLetterOffice;
        private readonly IDelayedDeliveryService _delayedDeliveryService;
        private readonly IDeliveryRetryStrategy _deliveryRetryStrategy;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        internal MulticastEventMessagePumpsFactory(ApplicationNameSetting applicationName,
                                                   InstanceNameSetting instanceName,
                                                   MaxDeliveryAttemptSetting maxDeliveryAttemptSetting,
                                                   IClock clock,
                                                   IDeadLetterOffice deadLetterOffice,
                                                   IDelayedDeliveryService delayedDeliveryService,
                                                   IDeliveryRetryStrategy deliveryRetryStrategy,
                                                   IDispatchContextManager dispatchContextManager,
                                                   IHandlerMapper handlerMapper,
                                                   ILogger logger,
                                                   IMessageDispatcherFactory messageDispatcherFactory,
                                                   INimbusMessagingFactory messagingFactory,
                                                   IRouter router,
                                                   ITypeProvider typeProvider)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _handlerMapper = handlerMapper;
            _logger = logger;
            _messageDispatcherFactory = messageDispatcherFactory;
            _messagingFactory = messagingFactory;
            _router = router;
            _typeProvider = typeProvider;
            _maxDeliveryAttemptSetting = maxDeliveryAttemptSetting;
            _deadLetterOffice = deadLetterOffice;
            _delayedDeliveryService = delayedDeliveryService;
            _deliveryRetryStrategy = deliveryRetryStrategy;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            var openGenericHandlerType = typeof (IHandleMulticastEvent<>);
            var handlerTypes = _typeProvider.MulticastEventHandlerTypes.ToArray();

            // Events are routed to Topics and we'll create a subscription per instance of the logical endpoint to enable multicast behaviour
            var allMessageTypesHandledByThisEndpoint = _handlerMapper.GetMessageTypesHandledBy(openGenericHandlerType, handlerTypes);
            var bindings = allMessageTypesHandledByThisEndpoint
                .Select(m => new {MessageType = m, TopicPath = _router.Route(m, QueueOrTopic.Topic)})
                .GroupBy(b => b.TopicPath)
                .Select(g => new
                             {
                                 TopicPath = g.Key,
                                 MessageTypes = g.Select(x => x.MessageType),
                                 HandlerTypes = g.SelectMany(x => _handlerMapper.GetHandlerTypesFor(openGenericHandlerType, x.MessageType))
                             })
                .ToArray();

            if (bindings.Any(b => b.MessageTypes.Count() > 1))
                throw new NotSupportedException("Routing multiple message types through a single Topic is not supported.");

            foreach (var binding in bindings)
            {
                foreach (var handlerType in binding.HandlerTypes)
                {
                    var messageType = binding.MessageTypes.Single();
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, _instanceName, handlerType);

                    _logger.Debug("Creating message pump for multicast event subscription '{0}/{1}' handling {2}", binding.TopicPath, subscriptionName, messageType);
                    var messageReceiver = _messagingFactory.GetTopicReceiver(binding.TopicPath, subscriptionName);

                    var handlerMap = new Dictionary<Type, Type[]> {{messageType, new[] {handlerType}}};
                    var pump = new MessagePump(_maxDeliveryAttemptSetting,
                                               _clock,
                                               _deadLetterOffice,
                                               _delayedDeliveryService,
                                               _deliveryRetryStrategy,
                                               _dispatchContextManager,
                                               _logger,
                                               _messageDispatcherFactory.Create(openGenericHandlerType, handlerMap), messageReceiver);
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