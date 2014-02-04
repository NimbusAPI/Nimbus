using System;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface INimbusMessageSenderFactory
    {
        INimbusMessageSender GetMessageSender(Type messageType);
        INimbusMessageSender GetMessageSender(string queuePath);
    }
}