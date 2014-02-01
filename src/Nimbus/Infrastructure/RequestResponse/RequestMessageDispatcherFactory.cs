using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    internal class RequestMessageDispatcherFactory
    {
        private MessagingFactory _messagingFactory;
        private IRequestBroker _requestBroker;

        public RequestMessageDispatcherFactory(MessagingFactory messagingFactory, IRequestBroker requestBroker)
        {
            _messagingFactory = messagingFactory;
            _requestBroker = requestBroker;
        }

        public IMessageDispatcher Create(Type messageType)
        {
            return new RequestMessageDispatcher(_messagingFactory, messageType, _requestBroker);
        }
    }
}