namespace Nimbus.Transports.AzureServiceBus2.Filtering
{
    using System.Linq;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal static class OrConditionSqlGenerator
    {
        public static string GenerateSqlFor(OrCondition condition)
        {
            var filterExpressions = condition.Conditions.Select(ConditionSqlGenerator.GenerateSqlFor)
                                             .Select(e => $"({e})")
                                             .ToArray();

            var filterExpression = string.Join(" OR ", filterExpressions);
            return filterExpression;
        }
    }
}