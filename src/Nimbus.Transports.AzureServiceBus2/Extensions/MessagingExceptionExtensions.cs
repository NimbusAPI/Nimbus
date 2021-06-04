namespace Nimbus.Transports.AzureServiceBus2.Extensions
{
    using Azure.Messaging.ServiceBus;

    internal static class MessagingExceptionExtensions
    {
        internal static bool IsTransientFault(this ServiceBusException exception)
        {
            return
                exception.Reason is ServiceBusFailureReason.ServiceTimeout ||
                exception.Reason is ServiceBusFailureReason.ServiceBusy ||
                exception.Reason is ServiceBusFailureReason.QuotaExceeded ||
                exception.Reason is ServiceBusFailureReason.MessagingEntityDisabled;
        }
    }
}