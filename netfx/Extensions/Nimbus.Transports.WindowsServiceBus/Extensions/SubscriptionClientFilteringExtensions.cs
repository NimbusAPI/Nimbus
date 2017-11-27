using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Transports.WindowsServiceBus.Extensions
{
    internal static class SubscriptionClientFilteringExtensions
    {
        internal static void ReplaceFilter(this SubscriptionClient subscriptionClient, string filterName, string filterExpression)
        {
            try
            {
                subscriptionClient.RemoveRule(filterName);
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            try
            {
                subscriptionClient.AddRule(filterName, new SqlFilter(filterExpression));
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
        }
    }
}