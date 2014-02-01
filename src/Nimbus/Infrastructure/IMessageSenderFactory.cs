using System;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface IMessageSenderFactory
    {
        INimbusMessageSender GetMessageSender(Type messageType);
    }
}