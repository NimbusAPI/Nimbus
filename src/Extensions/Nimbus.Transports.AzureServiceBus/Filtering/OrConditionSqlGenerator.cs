using System.Linq;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.Filtering
{
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