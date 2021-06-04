namespace Nimbus.Transports.AzureServiceBus2.Extensions
{
    using Azure.Messaging.ServiceBus;

    internal static class MessagingExceptionExtensions
    {
        internal static bool IsTransientFault(this ServiceBusFailureReason reason)
        {
            return
                reason is ServiceBusFailureReason.ServiceTimeout ||
                reason is ServiceBusFailureReason.ServiceBusy ||
                reason is ServiceBusFailureReason.QuotaExceeded ||
                reason is ServiceBusFailureReason.MessagingEntityDisabled;
        }
    }
}