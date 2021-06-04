namespace Nimbus.Transports.AzureServiceBus2.Extensions
{
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Azure.Messaging.ServiceBus.Administration;

    internal static class SubscriptionClientFilteringExtensions
    {
        internal static async Task ReplaceFilter(
            this ServiceBusAdministrationClient administrationClient,
            string filterName,
            string filterExpression,
            string topicName,
            string subscriptionName)
        {
            try
            {
                await administrationClient.DeleteRuleAsync(topicName, subscriptionName, filterName);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
            }

            try
            {
                var options = new CreateRuleOptions()
                              {
                                  Name = filterName,
                                  Filter = new SqlRuleFilter(filterExpression)
                              };
                await administrationClient.CreateRuleAsync(topicName, subscriptionName, options);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
            }
        }
    }
}