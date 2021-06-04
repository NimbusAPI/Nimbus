namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

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