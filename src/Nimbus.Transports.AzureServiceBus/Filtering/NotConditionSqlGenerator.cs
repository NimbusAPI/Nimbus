using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
    internal static class NotConditionSqlGenerator
    {
        public static string GenerateSqlFor(NotCondition condition)
        {
            var innerExpression = ConditionSqlGenerator.GenerateSqlFor(condition.ConditionToNegate);
            var filterExpression = $"NOT ({innerExpression})";
            return filterExpression;
        }
    }
}