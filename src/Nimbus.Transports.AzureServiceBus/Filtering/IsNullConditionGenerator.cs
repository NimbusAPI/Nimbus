using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal class IsNullConditionGenerator
    {
        public static string GenerateSqlFor(IsNullCondition condition)
        {
            return $"{condition.PropertyKey} IS NULL";
        }
    }
}