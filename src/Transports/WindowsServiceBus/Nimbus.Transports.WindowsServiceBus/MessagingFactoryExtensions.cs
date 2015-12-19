using System.Reflection;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Transports.WindowsServiceBus
{
    internal static class MessagingFactoryExtensions
    {
        private static readonly PropertyInfo _isFaultedProperty =
            typeof (MessagingFactory).GetProperty("IsFaulted", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly PropertyInfo _isClosedOrClosingProperty =
            typeof (MessagingFactory).GetProperty("IsClosedOrClosing", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        internal static bool IsBorked(this MessagingFactory messagingFactory)
        {
            var isFaulted = (bool) _isFaultedProperty.GetValue(messagingFactory);
            if (isFaulted) return true;

            var isClosedOrClosing = (bool) _isClosedOrClosingProperty.GetValue(messagingFactory);
            if (isClosedOrClosing) return true;

            return false;
        }
    }
}