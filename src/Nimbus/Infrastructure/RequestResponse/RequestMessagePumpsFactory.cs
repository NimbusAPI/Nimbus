using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class RequestMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly RequestMessageDispatcherFactory _dispatcherFactory;
        private readonly IQueueManager _queueManager;

        public RequestMessagePumpsFactory(ILogger logger,
                                          RequestHandlerTypesSetting requestHandlerTypes,
                                          RequestMessageDispatcherFactory dispatcherFactory,
                                          IQueueManager queueManager)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _dispatcherFactory = dispatcherFactory;
            _queueManager = queueManager;
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

                var queuePath = PathFactory.QueuePathFor(requestType);
                var messageReceiver = new NimbusQueueMessageReceiver(_queueManager, queuePath);
                var dispatcher = _dispatcherFactory.Create(requestType);
                var pump = new MessagePump(messageReceiver, dispatcher, _logger);

                yield return pump;
            }
        }
    }
}