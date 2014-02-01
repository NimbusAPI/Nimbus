using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class MulticastRequestMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly IMessageDispatcher _dispatcher;
        private readonly MessagingFactory _messagingFactory;
        private readonly ApplicationNameSetting _applicationName;

        public MulticastRequestMessagePumpsFactory(ILogger logger,
                                                   RequestHandlerTypesSetting requestHandlerTypes,
                                                   IMessageDispatcher dispatcher,
                                                   MessagingFactory messagingFactory,
                                                   ApplicationNameSetting applicationName)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _dispatcher = dispatcher;
            _messagingFactory = messagingFactory;
            _applicationName = applicationName;
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
                var messageReceiver = new NimbusMessageReceiver(_messagingFactory.CreateSubscriptionClient(topicPath, applicationSharedSubscriptionName));

                //FIXME still need to do this...
                //queueManager.EnsureSubscriptionExists(requestType, applicationSharedSubscriptionName);
                var pump = new MessagePump(messageReceiver, _dispatcher, _logger);
                yield return pump;
            }
        }
    }
}