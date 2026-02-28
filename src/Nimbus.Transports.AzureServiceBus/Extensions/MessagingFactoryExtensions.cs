using System.Reflection;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;

namespace Nimbus.Transports.AzureServiceBus.Extensions
{
    internal static class MessagingFactoryExtensions
    {
        private static readonly PropertyInfo _isFaultedProperty =
            typeof (BrokeredMessageFactory).GetProperty("IsFaulted", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly PropertyInfo _isClosedOrClosingProperty =
            typeof (BrokeredMessageFactory).GetProperty("IsClosedOrClosing", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        internal static bool IsBorked(this BrokeredMessageFactory messagingFactory)
        {
            var isFaulted = (bool) _isFaultedProperty.GetValue(messagingFactory);
            if (isFaulted) return true;

            var isClosedOrClosing = (bool) _isClosedOrClosingProperty.GetValue(messagingFactory);
            if (isClosedOrClosing) return true;

            return false;
        }

        internal static void Dispose(this BrokeredMessageFactory messagingFactory)
        {
            try
            {
                //messagingFactory.Close();
            }
            catch
            {
                // we don't care. It's already borked. We just want it to go away and release its connection.
            }
        }
    }
}