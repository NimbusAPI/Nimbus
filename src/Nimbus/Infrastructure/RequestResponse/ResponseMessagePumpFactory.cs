using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpFactory
    {
        private readonly PoorMansIoC _container;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly INimbusTransport _transport;
        private readonly ResponseMessageDispatcher _responseMessageDispatcher;

        internal ResponseMessagePumpFactory(ReplyQueueNameSetting replyQueueName,
                                            INimbusTransport transport,
                                            PoorMansIoC container,
                                            ResponseMessageDispatcher responseMessageDispatcher)
        {
            _replyQueueName = replyQueueName;
            _transport = transport;
            _responseMessageDispatcher = responseMessageDispatcher;
            _container = container;
        }

        public IMessagePump Create()
        {
            var pump = _container.ResolveWithOverrides<MessagePump>(_transport.GetQueueReceiver(_replyQueueName), _responseMessageDispatcher);
            return pump;
        }
    }
}