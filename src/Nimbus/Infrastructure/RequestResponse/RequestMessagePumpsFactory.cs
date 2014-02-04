using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessagePumpsFactory
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly RequestMessageDispatcherFactory _dispatcherFactory;
        private readonly IQueueManager _queueManager;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;

        public RequestMessagePumpsFactory(ILogger logger,
                                          RequestHandlerTypesSetting requestHandlerTypes,
                                          RequestMessageDispatcherFactory dispatcherFactory,
                                          IQueueManager queueManager,
                                          DefaultBatchSizeSetting defaultBatchSize)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _dispatcherFactory = dispatcherFactory;
            _queueManager = queueManager;
            _defaultBatchSize = defaultBatchSize;
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
                var pump = new MessagePump(messageReceiver, dispatcher, _logger, _defaultBatchSize);

                yield return pump;
            }
        }
    }
}