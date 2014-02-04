using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessageDispatcherFactory
    {
        private readonly IRequestBroker _requestBroker;
        private readonly INimbusMessageSenderFactory _messageSenderFactory;

        public RequestMessageDispatcherFactory(IRequestBroker requestBroker, INimbusMessageSenderFactory messageSenderFactory)
        {
            _requestBroker = requestBroker;
            _messageSenderFactory = messageSenderFactory;
        }

        public IMessageDispatcher Create(Type messageType)
        {
            return new RequestMessageDispatcher(_messageSenderFactory, messageType, _requestBroker);
        }
    }
}