using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Nimbus.Transports.AzureServiceBus.Extensions
{
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