using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public interface IMessageSenderFactory
    {
        MessageSender GetMessageSender(Type messageType);
    }
}