using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface IMessageSenderFactory
    {
        MessageSender GetMessageSender(Type messageType);
    }
}