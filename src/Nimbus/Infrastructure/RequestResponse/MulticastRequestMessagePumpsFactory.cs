using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly ApplicationNameSetting _applicationName;
        private readonly IQueueManager _queueManager;
        private readonly IMulticastRequestBroker _multicastRequestBroker;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly BatchReceiveTimeoutSetting _batchReceiveTimeout;

        public MulticastRequestMessagePumpsFactory(ILogger logger,
                                                   RequestHandlerTypesSetting requestHandlerTypes,
                                                   ApplicationNameSetting applicationName,
                                                   IQueueManager queueManager,
                                                   IMulticastRequestBroker multicastRequestBroker,
                                                   DefaultBatchSizeSetting defaultBatchSize,
                                                   INimbusMessagingFactory messagingFactory,
                                                   IClock clock,
                                                   BatchReceiveTimeoutSetting batchReceiveTimeout)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _applicationName = applicationName;
            _queueManager = queueManager;
            _multicastRequestBroker = multicastRequestBroker;
            _defaultBatchSize = defaultBatchSize;
            _messagingFactory = messagingFactory;
            _clock = clock;
            _batchReceiveTimeout = batchReceiveTimeout;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating multicast request message pumps");

            var requestTypes = _requestHandlerTypes.Value.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleRequest<,>)))
                                                   .Select(gi => gi.GetGenericArguments().First())
                                                   .OrderBy(t => t.FullName)
                                                   .Distinct()
                                                   .ToArray();

            foreach (var requestType in requestTypes)
            {
                _logger.Debug("Creating message pump for multicase request type {0}", requestType.Name);

                var topicPath = PathFactory.TopicPathFor(requestType);
                var applicationSharedSubscriptionName = String.Format("{0}", _applicationName);

                var messageReceiver = new NimbusSubscriptionMessageReceiver(_queueManager, topicPath, applicationSharedSubscriptionName, _batchReceiveTimeout);
                _garbageMan.Add(messageReceiver);

                var dispatcher = new MulticastRequestMessageDispatcher(_messagingFactory, _multicastRequestBroker, requestType);
                _garbageMan.Add(dispatcher);

                var pump = new MessagePump(messageReceiver, dispatcher, _logger, _defaultBatchSize, _clock);
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