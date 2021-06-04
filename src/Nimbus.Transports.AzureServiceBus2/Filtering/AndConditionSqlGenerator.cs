namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using System.Linq;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal static class AndConditionSqlGenerator
    {
        public static string GenerateSqlFor(AndCondition condition)
        {
            var filterExpressions = condition.Conditions.Select(ConditionSqlGenerator.GenerateSqlFor)
                                             .Select(e => $"({e})")
                                             .ToArray();

            var filterExpression = string.Join(" AND ", filterExpressions);
            return filterExpression;
        }
    }
}