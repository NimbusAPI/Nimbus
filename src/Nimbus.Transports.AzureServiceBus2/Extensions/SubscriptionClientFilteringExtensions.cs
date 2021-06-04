namespace Nimbus.Transports.AzureServiceBus2.Extensions
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    internal static class SubscriptionClientFilteringExtensions
    {
        internal static async Task ReplaceFilter(this SubscriptionClient subscriptionClient, string filterName, string filterExpression)
        {
            try
            {
                await subscriptionClient.RemoveRuleAsync(filterName);
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            try
            {
                await subscriptionClient.AddRuleAsync(filterName, new SqlFilter(filterExpression));
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
        }
    }
}