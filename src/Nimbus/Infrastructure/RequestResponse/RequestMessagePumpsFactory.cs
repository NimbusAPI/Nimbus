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
    internal class RequestMessagePumpsFactory : ICreateComponents
    {
        private readonly ILogger _logger;
        private readonly RequestHandlerTypesSetting _requestHandlerTypes;
        private readonly IQueueManager _queueManager;
        private readonly IRequestHandlerFactory _requestHandlerFactory;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IClock _clock;

        private readonly GarbageMan _garbageMan = new GarbageMan();
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;

        public RequestMessagePumpsFactory(ILogger logger,
                                          RequestHandlerTypesSetting requestHandlerTypes,
                                          IQueueManager queueManager,
                                          IRequestHandlerFactory requestHandlerFactory,
                                          INimbusMessagingFactory messagingFactory,
                                          IClock clock,
                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit)
        {
            _logger = logger;
            _requestHandlerTypes = requestHandlerTypes;
            _queueManager = queueManager;
            _requestHandlerFactory = requestHandlerFactory;
            _messagingFactory = messagingFactory;
            _clock = clock;
            _concurrentHandlerLimit = concurrentHandlerLimit;
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
                var messageReceiver = new NimbusQueueMessageReceiver(_queueManager, queuePath, _concurrentHandlerLimit);
                _garbageMan.Add(messageReceiver);

                var dispatcher = new RequestMessageDispatcher(_messagingFactory, requestType, _requestHandlerFactory, _clock);
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