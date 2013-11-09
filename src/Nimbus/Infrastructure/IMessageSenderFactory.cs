using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IMessageSenderFactory
    {
        MessageSender GetMessageSender(Type messageType);
    }
}