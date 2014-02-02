using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessageDispatcherFactory
    {
        private readonly IQueueManager _queueManager;
        private readonly IRequestBroker _requestBroker;

        public RequestMessageDispatcherFactory(IQueueManager queueManager, IRequestBroker requestBroker)
        {
            _queueManager = queueManager;
            _requestBroker = requestBroker;
        }

        public IMessageDispatcher Create(Type messageType)
        {
            return new RequestMessageDispatcher(_queueManager, messageType, _requestBroker);
        }
    }
}