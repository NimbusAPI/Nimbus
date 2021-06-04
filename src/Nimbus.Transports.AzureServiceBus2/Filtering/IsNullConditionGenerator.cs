namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal class IsNullConditionGenerator
    {
        public static string GenerateSqlFor(IsNullCondition condition)
        {
            return $"{condition.PropertyKey} IS NULL";
        }
    }
}