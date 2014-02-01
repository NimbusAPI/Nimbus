using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class RequestMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly RequestMessageDispatcherFactory _dispatcherFactory;
        private readonly MessagingFactory _messagingFactory;

        public RequestMessagePumpsFactory(ILogger logger, RequestHandlerTypesSetting requestHandlerTypes, RequestMessageDispatcherFactory dispatcherFactory, MessagingFactory messagingFactory)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _dispatcherFactory = dispatcherFactory;
            _messagingFactory = messagingFactory;
        }

        public IEnumerable<IMessagePump> CreateAll()
        {
            _logger.Debug("Creating request message pumps");

            var requestTypes = _requestHandlerTypes.Value.SelectMany(ht => ht.GetGenericInterfacesClosing(typeof (IHandleRequest<,>)))
                                                   .Select(gi => gi.GetGenericArguments().First())
                                                   .OrderBy(t => t.FullName)
                                                   .Distinct()
                                                   .ToArray();

            foreach (var requestType in requestTypes)
            {
                _logger.Debug("Creating message pump for request type {0}", requestType.Name);

                var queueName = PathFactory.QueuePathFor(requestType);
                var messageReceiver = new NimbusMessageReceiver(_messagingFactory.CreateMessageReceiver(queueName));
                var dispatcher = _dispatcherFactory.Create(requestType);
                var pump = new MessagePump(messageReceiver, dispatcher, _logger);

                yield return pump;
            }
        }
    }
}