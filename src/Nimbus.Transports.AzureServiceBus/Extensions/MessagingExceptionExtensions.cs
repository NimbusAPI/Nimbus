using System;
using Microsoft.Azure.ServiceBus;

namespace Nimbus.Transports.AzureServiceBus.Extensions
{
    internal static class MessagingExceptionExtensions
    {
        internal static bool IsTransientFault(this Exception exception)
        {
            // Refer to: http://msdn.microsoft.com/en-us/library/hh418082.aspx
            return
                exception is TimeoutException || // Retry might help in some cases; add retry logic to code.
                exception is ServerBusyException || // Client may retry after certain interval. If a retry results in a different exception, check retry behavior of that exception.
                exception is MessagingCommunicationException || // Retry might help if there are intermittent connectivity issues.
                exception is QuotaExceededException || // Retry might help if messages have been removed in the meantime.
                exception is MessagingEntityDisabledException; // Retry might help if the entity has been activated in the interim.
        }
    }
}