using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly ApplicationNameSetting _applicationName;
        private readonly IQueueManager _queueManager;
        private readonly IMulticastRequestHandlerFactory _multicastRequestHandlerFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();

        public MulticastRequestMessagePumpsFactory(ILogger logger,
                                                   RequestHandlerTypesSetting requestHandlerTypes,
                                                   ApplicationNameSetting applicationName,
                                                   IQueueManager queueManager,
                                                   IMulticastRequestHandlerFactory multicastRequestHandlerFactory,
                                                   INimbusMessagingFactory messagingFactory,
                                                   IClock clock)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _applicationName = applicationName;
            _queueManager = queueManager;
            _multicastRequestHandlerFactory = multicastRequestHandlerFactory;
            _messagingFactory = messagingFactory;
            _clock = clock;
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

                var messageReceiver = new NimbusSubscriptionMessageReceiver(_queueManager, topicPath, applicationSharedSubscriptionName);
                _garbageMan.Add(messageReceiver);

                var dispatcher = new MulticastRequestMessageDispatcher(_messagingFactory, _multicastRequestHandlerFactory, requestType, _clock);
                _garbageMan.Add(dispatcher);

                var pump = new MessagePump(messageReceiver, dispatcher, _logger, _clock);
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