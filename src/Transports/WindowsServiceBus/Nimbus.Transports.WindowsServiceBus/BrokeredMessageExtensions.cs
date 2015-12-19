using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    internal static class BrokeredMessageExtensions
    {
        internal static string SafelyGetBodyTypeNameOrDefault(this BrokeredMessage message)
        {
            object name;
            return (message.Properties.TryGetValue(MessagePropertyKeys.MessageType, out name) ? (string)name : default(string));
        }

    }
}