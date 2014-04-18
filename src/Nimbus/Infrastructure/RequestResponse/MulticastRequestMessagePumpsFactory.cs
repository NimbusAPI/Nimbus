using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly ApplicationNameSetting _applicationName;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly IDependencyResolver _dependencyResolver;

        public MulticastRequestMessagePumpsFactory(ILogger logger,
                                                   RequestHandlerTypesSetting requestHandlerTypes,
                                                   ApplicationNameSetting applicationName,
                                                   INimbusMessagingFactory messagingFactory,
                                                   IBrokeredMessageFactory brokeredMessageFactory,
                                                   IClock clock,
                                                   IDependencyResolver dependencyResolver)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _applicationName = applicationName;
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            foreach (var handlerType in _requestHandlerTypes.Value)
            {
                var requestTypes = handlerType.GetGenericInterfacesClosing(typeof (IHandleRequest<,>))
                                              .Select(gi => gi.GetGenericArguments().First())
                                              .OrderBy(t => t.FullName)
                                              .Distinct()
                                              .ToArray();

                foreach (var requestType in requestTypes)
                {
                    var topicPath = PathFactory.TopicPathFor(requestType);
                    var subscriptionName = PathFactory.SubscriptionNameFor(_applicationName, handlerType);

                    _logger.Debug("Creating message pump for multicast request {0}/{1}", topicPath, subscriptionName);

                    var messageReceiver = _messagingFactory.GetTopicReceiver(topicPath, subscriptionName);

                    var dispatcher = new RequestMessageDispatcher(_messagingFactory, _brokeredMessageFactory, requestType, _clock, _logger, _dependencyResolver, handlerType);
                    _garbageMan.Add(dispatcher);

                    var pump = new MessagePump(messageReceiver, dispatcher, _logger, _clock);
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